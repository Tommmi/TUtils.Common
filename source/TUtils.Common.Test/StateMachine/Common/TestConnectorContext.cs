using TUtils.Common.Test.StateMachine.States;

namespace TUtils.Common.Test.StateMachine.Common;

public class TestConnectorContext
{
    public TestConnectorStateInitial TestConnectorStateInitial { get; private set; }

    public TestConnectorStateConnecting TestConnectorStateConnecting { get; private set; }

    public TestConnectorStateConnected TestConnectorStateConnected { get; private set; }

    public TestConnectorStateReconnecting TestConnectorStateReconnecting { get; private set; }


    public string ClientId { get; private set; }

    public IConnectableService ConnectableService { get; private set; }


    public TestConnectorContext(string clientId,
                             IConnectableService connectableService)
    {
        ClientId = clientId;
        connectableService = connectableService;
    }

    public void Initialize(TestConnectorStateInitial connectionStateInitial,
                             TestConnectorStateConnecting connectionStateConnecting,
                             TestConnectorStateConnected connectionStateConnected,
                             TestConnectorStateReconnecting connectionStateReconnecting)
    {
	    TestConnectorStateInitial = connectionStateInitial;
	    TestConnectorStateConnecting = connectionStateConnecting;
	    TestConnectorStateConnected = connectionStateConnected;
	    TestConnectorStateReconnecting = connectionStateReconnecting;
    }
}

