# TUtils.Common
TUtils.Common.dll is a basic shared .NET-library for all projects based on the open TUtils-framework.
TUtils.Common.EF6.dll contains some usefull tools with entity data framework 6
TUtils.Common.MVC.dll contains some usefull extensions for ASP.NET MVC

## History
see [history](documentation/history.md)

## Contents

### TUtils.Common.dll

#### Requirements
NuGet packages: 
	- log4net

#### Classes and Interfaces

| Class/Interface  | Description |
| ------ | ----------- |
| **Logging Framework** |  |
| ILogWriter     | System interface for pluggable log writers |
| Log4NetWriter       | Log4Net implementation of ILogWriter |
| LogConsoleWriter | Console implementation of ILogWriter for debug output |
| LoggerExtension | this.Log().Log... Syntax |
| **File System & Path Utilities** |  |
| TFilePath     | File path manipulation with parsing into components and static helper methods for file operations |
| CommandLineArgs       | Command line argument parsing with type-safe definitions and automatic USAGE text generation |
| **Async & Threading** |  |
| AsyncEvent, AsyncEvent\<TResult\> | Events for asynchronous waiting with await keyword and optional result values |
| AsyncThreadStarter       | Creates threads with async/await support and cancellation token handling |
| TThreadStorage\<T\>       | Thread-bound object storage for thread-local data |
| **Data Structures** |  |
| IndexedTable\<T1,T2\>...       | Performance-optimized indexed collections with automatic dictionary creation for fast lookups by multiple keys |
| **Security & Cryptography** |  |
| ICertificateProvider, CertificateProvider | Asymmetric RSA certificate management - loading from files, Windows store, or Base64 strings [see here](#asymmetric-encryption-rsa-and-certificates) |
| ISymmetricCryptProvider, SymmetricCryptProvider | AES symmetric encryption provider for text and binary data  [see here](#cryptographic-utilities)  |
| **Utilities** |  |
| IUniqueTimeStampCreator, UniqueTimeStampCreator | Thread-safe unique timestamp generation |
| ISystemTimeProvider, SystemTimeProvider | System time abstraction (useful for testing with mocks) |
| IDebouncer, Debouncer | Signal debouncing - prevents rapid repeated signal handling |
| IDIContainer | Technology-independent dependency injection container abstraction |
| ILazy, Lazy | Lazy loading abstraction with DI container integration |
| ITransactionService | Transaction management abstraction |
| AssemblyLib | Utility library for searching and manipulating assemblies |

## Logging with Extension Methods

The TUtils.Common library provides a modern, flexible logging framework through extension methods that integrate seamlessly with any object and support task-based context propagation.

### Core Logging Extension Methods

The `LoggerExtension` class provides extension methods that enable a convenient `this.Log()` syntax for logging from any object:

```csharp
using TUtils.Common.Logging;

// Basic logging from any object
this.Log().LogInfo(map: () => new { message = "Hello World", userId = 123 });
this.Log().LogError(e: exception);
this.Log().LogWarning(message: "Warning message");

// Set context values that persist across tasks
this.Log().SetLoggingValue(valueName: "userId", value: "12345");
this.Log().SetLoggingValue(valueName: "requestId", value: Guid.NewGuid().ToString());
```

### Initialization Options

Before using the logging extensions, you must initialize the logging framework with one of these methods:

**Console Logging:**
```csharp
// Initialize console logging with minimum severity level
this.InitializeConsoleLogging(minLogSeverityEnum: LogSeverityEnum.INFO);
```

**Log4Net Integration:**
```csharp
// Initialize Log4Net integration (requires log4net.config)
this.InitializeLog4NetLogging();
```

### Task-Based Context Propagation

A key feature of the logging extensions is automatic context propagation across task boundaries:

```csharp
// Set context values in main thread
this.InitializeConsoleLogging(minLogSeverityEnum: LogSeverityEnum.INFO);
this.Log().SetLoggingValue(valueName: "sessionId", value: "abc123");

// Context automatically propagates to background tasks
await Task.Run(() => 
{
    // The sessionId context value is still available here
    this.Log().LogInfo(map: () => new { message = "Background task executed" });
});
```

### Task Exception Logging

The framework includes specialized extension methods for Task exception handling:

```csharp
using System.Threading.Tasks; // TaskExtension namespace

// Automatically log exceptions from tasks without catching them
await SomeAsyncOperation().LogExceptions();
await SomeAsyncOperationWithResult<string>().LogExceptions();
```

These methods log exceptions when tasks fail but still allow the exceptions to propagate, providing visibility into async operation failures without affecting error handling flow.

### Key Features

- **Universal Logging:** Any object can call `this.Log()` to get a logger instance
- **Context Propagation:** Logging context values automatically flow across task boundaries using `TaskStorage<ICallerContext>`
- **Structured Logging:** Support for structured data through lambda expressions `map: () => new { ... }`
- **Multiple Backends:** Console and Log4Net implementations available
- **Thread-Safe:** Built-in synchronization for context management and logger initialization
- **Exception Handling:** Specialized extension methods for async operation exception logging

## Cryptographic Utilities

The TUtils.Common library provides comprehensive cryptographic functionality for both symmetric and asymmetric encryption.

### Symmetric Encryption (AES)

**Interface:** `ISymmetricCryptProvider` and `ISymmetricCrypt`  
**Implementation:** `SymmetricCryptProvider`

Symmetric encryption uses the AES algorithm and enables encryption and decryption of text and binary data with a shared key.

#### Usage:

```csharp
using TUtils.Common.Security.Symmetric;
using TUtils.Common.Security.Symmetric.AesCryptoServiceProvider;
using TUtils.Common.Security.Symmetric.Common;

// Create provider
var symmetricCryptProvider = new SymmetricCryptProvider() as ISymmetricCryptProvider;

// Encryption with automatically generated key
ISymmetricSecret secret = null;
EncryptedText encryptedText = null;
string plainText = "My secret text";

using (var symmetricCrypt = symmetricCryptProvider.Create())
{
    secret = symmetricCrypt.Secret;  // Store key for later use
    encryptedText = symmetricCrypt.Encrypt(new PlainText(plainText));
}

// Decryption with existing key
using (var symmetricCrypt = symmetricCryptProvider.Create(secret))
{
    var decryptedText = symmetricCrypt.Decrypt(encryptedText);
    Console.WriteLine(decryptedText.Text); // "My secret text"
}

// Encrypt binary data
byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes("Binary data");
using (var symmetricCrypt = symmetricCryptProvider.Create(secret))
{
    var encryptedData = symmetricCrypt.Encrypt(new PlainData(plainBytes));
    var decryptedData = symmetricCrypt.Decrypt(encryptedData);
    // decryptedData.Data contains the original bytes
}
```

### Asymmetric Encryption (RSA) and Certificates

**Interface:** `ICertificateProvider`, `IPublicCertificate`, `IPrivateCertificate`  
**Implementation:** `CertificateProvider`

Asymmetric encryption uses RSA certificates for encryption, decryption, and digital signatures.

#### Usage:

```csharp
using TUtils.Common.Security.Asymmetric;
using TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider;
using TUtils.Common.Security.Asymmetric.Common;

var certificateProvider = new CertificateProvider() as ICertificateProvider;

// Load certificate from file
using (var publicCert = certificateProvider.GetPublicCertificateByFilePath(@"C:\cert\public.cer", "password"))
using (var privateCert = certificateProvider.GetPrivateCertificateByFilePath(@"C:\cert\private.pfx", "password"))
{
    // Encrypt text with public key
    string plainText = "Secret text";
    string encryptedBase64 = publicCert.Encrypt(plainText);
    
    // Decrypt text with private key
    string decrypted = privateCert.Decrypt(encryptedBase64);
    
    // Create digital signature
    string signature = privateCert.SignBase64(plainText);
    
    // Verify signature
    bool isValid = publicCert.Verify(plainText, signature);
}

// Certificate from Windows certificate store
using (var publicCert = certificateProvider.GetPublicCertificateFromWindowsStorage("MyCertificate"))
using (var privateCert = certificateProvider.GetPrivateCertificateFromWindowsStorage("MyCertificate"))
{
    // Usage analogous to file-based certificates
}

// Certificate from Base64 string
var publicCertBase64 = certificateProvider.CreatePublicCertificateDefinitionByString("MIIC7jCCA...");
var privateCertBase64 = certificateProvider.CreatePrivateCertificateDefinitionByString("MIIJuQIBA...");

using (var publicCert = publicCertBase64.GetPublicCertificate("password"))
using (var privateCert = privateCertBase64.GetPrivateCertificate())
{
    // Normal encryption/decryption and signature operations
}
```

#### Important Notes:

1. **Memory Management:** All certificate objects implement `IDisposable` and must be disposed using `using` or manually
2. **Key Compatibility:** Public and private keys must belong together as a pair
3. **Data Sizes:** RSA can only encrypt limited amounts of data (depending on key size)
4. **Security:** Private keys should be password-protected and stored securely
5. **Performance:** For large amounts of data, symmetric encryption should be used

#### Typical Use Cases:

- **Symmetric Encryption:** Large amounts of data, file encryption, session keys
- **Asymmetric Encryption:** Key exchange, small sensitive data, digital signatures
- **Hybrid Approach:** RSA for key exchange, AES for data encryption
|        |             |

## Extensions

| Extended Type | Extension Method | Description |
| ------------- | ---------------- | ----------- |
| **String Extensions** | | **Most commonly used string utilities** |
| string | IsNullOrEmpty() | Checks if string is null or empty |
| string | IsEmpty() | Checks if string is empty (not null) |
| string | EqualsIgnoreCase(string) | Case-insensitive string comparison |
| string | ContainsIgnoreCase(string) | Case-insensitive substring search |
| string | ToInt(int defaultValue) | Safe string to integer conversion with fallback |
| string | Left(int count), Right(int count) | Extract substring from start/end |
| string | RemoveLeft(int), RemoveRight(int) | Remove characters from start/end |
| string | RemoveWhitespaces() | Remove all spaces and tabs |
| string | Combine(string path2) | Path.Combine wrapper for file paths |
| string | ToHex() | Convert numeric values to hex strings |
| string | FromHexStringToByteArray() | Convert hex string to byte array |
| string | FromAsciiStringToByteArray() | Convert ASCII string to byte array |
| string | ToUTF8CodedByteArray() | Convert string to UTF8 byte array |
| string | ToByteArrayFromBase64String() | Convert Base64 string to byte array |
| string | DeserializeByTUtils\<T\>() | JSON serialization/deserialization |
| **Collection Extensions** | | **Array and collection utilities** |
| T[] | Append(params T[] items) | Concatenate arrays |
| IEnumerable\<T\> | IsNullOrEmpty(), IsNotNullOrEmpty() | Collection null/empty validation |
| IEnumerable\<T\> | ForEach(Action\<T\>) | Functional iteration over collections |
| IEnumerable\<T\> | AreEquals(IEnumerable\<T\>) | Deep equality comparison for collections |
| IList\<T\> | RemoveWhere(Func\<T, bool\>) | Conditional item removal |
| List\<T\> | SetRange(IEnumerable\<T\>) | Clear and replace list contents |
| **Type & Object Extensions** | | **Type conversion and reflection** |
| object | TryConvertToType\<T\>(out T, T defaultValue) | Universal type conversion with fallback |
| object | SerializeByTUtils() | Object serialization to JSON |
| Type | HasInterface(Type interfaceType) | Check if type implements interface |
| int,long,string,etc | AsEnum\<TEnum\>(TEnum unknownValue) | Safe enum conversion |
| **Byte Array Extensions** | | **Binary data manipulation** |
| byte[] | AreEqual(byte[] other) | Byte array comparison |
| byte[] | ConcatBytes(byte[] other) | Byte array concatenation |
| byte[] | SubBytes(int startIdx, int count) | Byte array slicing |
| byte[] | ToBase64String() | Convert byte array to Base64 string |
| byte[] | ToStringFromUTF8CodedByteArray() | Convert UTF8 byte array to string |
| **Exception Extensions** | | **Error handling utilities** |
| Exception | DumpException() | Create detailed exception log entry |
| Exception | FindInnerException\<T\>() | Find specific inner exception type |
| **Task Extensions** | | **Async/await utilities** |
| Task | LogExceptions() | Log task exceptions without catching |
| Task\<T\> | WaitAndGetResult(CancellationToken) | Synchronous task result retrieval |
| **Reflection Extensions** | | **Compile-time safe reflection** |
| Expression | GetMemberName\<T, TMemberType\>() | Extract member names at compile time |











