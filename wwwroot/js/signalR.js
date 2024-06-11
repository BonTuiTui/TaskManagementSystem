<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/5.0.11/signalr.min.js"></script>
<script>
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .build();

    connection.on("ReceiveNotification", function (message) {
        alert(message); // Popup thông báo
    });

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });
</script>