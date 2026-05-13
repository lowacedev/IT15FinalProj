// SignalR Initialization Script
// Exports the configured connection for other modules to use

const getUserId = () => {
    const metaTag = document.querySelector('meta[name="signalr-userid"]');
    return metaTag ? metaTag.getAttribute('content') : null;
};

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Handle connection lifecycle
connection.onreconnecting(error => {
    console.assert(connection.state === signalR.HubConnectionState.Reconnecting);
    console.warn(`Connection lost due to error "${error}". Reconnecting.`);
});

connection.onreconnected(connectionId => {
    console.assert(connection.state === signalR.HubConnectionState.Connected);
    console.log(`Connection reestablished. Connected with connectionId "${connectionId}".`);
    
    // Rejoin group on reconnect
    const userId = getUserId();
    if (userId) {
        connection.invoke("JoinUserGroup", userId).catch(err => console.error(err.toString()));
    }
});

connection.onclose(error => {
    console.assert(connection.state === signalR.HubConnectionState.Disconnected);
    console.error(`Connection closed due to error "${error}".`);
});

// Start the connection
const startSignalR = async () => {
    try {
        await connection.start();
        console.log("SignalR Connected.");
        
        const userId = getUserId();
        if (userId) {
            await connection.invoke("JoinUserGroup", userId);
            console.log(`Joined notification group for user: ${userId}`);
        } else {
            console.warn("SignalR User ID meta tag not found. Did not join a user group.");
        }
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        setTimeout(startSignalR, 5000);
    }
};

// Start immediately when the script loads
startSignalR();

// Export the connection so other scripts can attach event listeners
export default connection;
