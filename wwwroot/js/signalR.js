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

function showToast(message) {
    // Create toast element
    var toast = document.createElement('div');
    toast.className = 'toast';
    toast.role = 'alert';
    toast.ariaLive = 'assertive';
    toast.ariaAtomic = 'true';

    toast.innerHTML = `
            <div class="toast-header">
                <i class="bi bi-person-vcard me-2"></i>
                <strong class="me-auto">Task Notification</strong>
                <small class="text-muted">just now</small>
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
    var bootstrapToast = new bootstrap.Toast(toast);
    bootstrapToast.show();

    // Remove the toast element after it is hidden
    toast.addEventListener('hidden.bs.toast', function () {
        toast.remove();
    });
}