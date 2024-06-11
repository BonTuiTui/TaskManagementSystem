const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", function (message) {
    showToast(message);
    updateUnreadNotificationsCount();
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

function timeAgo(date) {
    const now = new Date();
    const secondsPast = (now.getTime() - date.getTime()) / 1000;

    if (secondsPast < 60) {
        return 'just now';
    }
    if (secondsPast < 3600) {
        const minutes = Math.floor(secondsPast / 60);
        return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
    }
    if (secondsPast <= 86400) {
        const hours = Math.floor(secondsPast / 3600);
        return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    }
    const days = Math.floor(secondsPast / 86400);
    return `${days} day${days > 1 ? 's' : ''} ago`;
}

function showToast(message) {
    // Create toast element
    var toast = document.createElement('div');
    toast.className = 'toast';
    toast.role = 'alert';
    toast.ariaLive = 'assertive';
    toast.ariaAtomic = 'true';

    const now = new Date();
    const timeAgoText = timeAgo(now);

    toast.innerHTML = `
            <div class="toast-header">
                <i class="bi bi-person-vcard me-2"></i>
                <strong class="me-auto">Task Notification</strong>
                <small class="text-muted">${timeAgoText}</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;

    // Append toast to container
    var container = document.getElementById('toast-container');
    container.appendChild(toast);

    // Initialize and show toast
    var bootstrapToast = new bootstrap.Toast(toast, { delay: 60000 }); // delay in milliseconds
    bootstrapToast.show();

    // Remove the toast element after it is hidden
    toast.addEventListener('hidden.bs.toast', function () {
        toast.remove();
    });

}