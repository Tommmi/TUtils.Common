# TUtils.Common
TUtils.Common.dll is a basic shared .NET-library for all projects based on the open TUtils-framework.
TUtils.Common.EF6.dll contains some useful tools with entity data framework 6

## History
see [history](documentation/history.md)

## Contents

| Class  | Description |
| ------ | ----------- |
| AsyncEvent | An event for which a listener can wait asynchronously with key word await |
| AsyncThreadStarter       | Creates Threads, for which you can wait asynchronously and which you can cancel via cancellation token            |
| CommandLineArgs       | represents command line arguments. Analize the command line arguments and provides USAGE text  |
| IndexedTable       |  A set of tuples, that provides performance optimized Find-methods to it's values. <br>With "IndexedTable" you needn't waste time in programming dictionaries for fast access to values by different keys. "IndexedTable" will care for instantiating such dictionaries on-the-fly. It will also keep them up to date. <br>Do you want to use a compound key ? Don't worry. "IndexedTable" will use compound hash keys automatically if neccessary.   |
| TFilePath     |  Represents a file path. Splits a file path into <br><ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]. <br>Provides some static helper methods for file and folder manipulation      |
| TThreadStorage       | Represents an object, which is bound to the current thread.   |
| ITLog       | programming interface for logging  |
| TLog       | TLog is the recommended implementation of ITLog, but is independent of the underlying logging technology. With TLog you may use log4Net as well as any other technology to write the concrete logging statements. TLog needs to be initialized with a concrete log writer implementing the interface ILogWriter. There are two suggested implementations of ILogWriter shipped with this library: Log4NetWriter and LogMoc. Usage:<br>`ITLog logger = new TLog(new Log4NetWriter(),isLoggingOfMethodNameActivated:false);`<br>`logger.LogInfo(this,"myComplexObject={0} !",()=>myComplexObject.ToString());`  |
| ILogWriter     | system interface for logging  |
| Log4NetWriter       |  Log4Net implementation of ILogWriter           |
| LogMocWriter       |  Empty implementation of ILogWriter (advice: better use a mocking framework) |
| LogConsoleWriter |  implementation of ILogWriter. Writes loggng output to debug Console |
| AssemblyLib       | library for searching through all assemblies            |
| ICertificateProvider    |  everything for handling assymetric certificates - recommended implementation is new CertificateProvider()  |
| ISymmetricCryptProvider   | everything for handling symmetric cryptography - recommended implementation is new SymmetricCryptProvider() |
| IUniqueTimeStampCreator        | a timestamp creator which creates process-unique timestamps            |
| IDIContainer | interface for a technology independent abstraction of a dependency injection container |
|        |             |

### Extensions

| Extended Type | Extension Method | Description |
| ------------- | ---------------- | ----------- |
| T[] | Append |  |
| Exception | DumpException |  |
| Exception | FindInnerException |  |
| object | TryConvertToType | converts: <br>- string => enum (flags) <br>- enum (flags) => number <br>- number => enum (flags) <br>- string => byte[] (ASCII) (see also StringExtension.FromASCIIStringToByteArray()) <br>- byte[] (ASCII) => string  |
| string | Combine |  |
| string | CutRight |  |
| string | FromHexStringToByteArray |  |
| string | FromAsciiStringToByteArray |  |
| string | RemoveRight |  |
| string | RemoveLeft |  |
| string | Left |  |
| string | Right |  |
| string | ContainsIgnoreCase |  |
| string | Remove(pattern) |  |
| string | RemoveController(this string obj) | removes the substring "Controller" |
| string | TryConvertDecToLong |  |
| string | TryConvertHexToLong |  |
| string | ToUTF8CodedByteArray |  |
| string | ToUnderlyingByteArray |  |
| string | ToByteArrayFromBase64String |  |
| int,long,string,.. | AsEnum<TEnum> |  |
| IList | RemoveWhere |  |
| List | SetRange |  |
| IEnumerable | AreEquals(IEnumerable<T> enumeration) |  |
| IEnumerable | IsNullOrEmpty |  |
| Type | HasInterface(Type interfaceType) |  |
| byte[] | AreEqual(byte[] other) |  |
| byte[] | ConcatBytes |  |
| byte[] | SubBytes |  |
| byte[] | ToBase64String |  |
| ushort, uint | ToByteArray |  |
| double | ToInt32 |  |
| Task | LogExceptions(ITLog logger) |  |

### TUtils.Common.EF6.dll
| Class  | Description |
| ------ | ----------- |
| ITransactionService<TDbContext> | provides DbContext and encapsulates actions with a transaction. |
| TransactionService<TDbContext> | default implementation of ITransactionService |
| IDbContextFactory<TDbContext> | factory for DbContext |
| DbContextFactory<TDbContext> | default implementation of IDbContextFactory |
| DbContextFactory4Unittest<TDbContext> | special implementation of IDbContextFactory for automatic component tests. **Works without configuration file app.config or web.config.** Enables programmer to run DAL-tests against SQL Server LocalDb within an unit test environment |
|  |  |
|  |  |

### TUtils.Common.MVC.dll
#### Classes
| Class  | Description |
| ------ | ----------- |
| ExtendedUnityContainer | implementation of the abstract dependency injection container interface IDIConatiner by Unity. Extends Unity by some useful methods. |
| MustBeAuthorized | Applied to an action or a controller [MustBeAuthorized]  is an [AuthorizeAttribute] which redirects unauthorized requests to AccountController.Login(string returnUrl). Assumes there is such an action. |
|  |  |
|  |  |
|  |  |

#### Extensions
| Extended Type | Extension Method | Description |
| ------------- | ---------------- | ----------- |
| HtmlHelper<TModel> | RenderFormGroup(model=>model.MyProperty) | creates form group |
|  |  |  |
|  |  |  |












