using System.Threading.Tasks;

namespace TUtils.Common.Test.StateMachine.Common;

public interface IConnectableService
{
    Task<bool> TryConnectOneTime();

    Task<bool> TryReconnect();
}
