$(document).ready(function () {
    // Load notifications when the modal is opened
    $('#notificationLink').on('click', function () {
        var userId = window.userId; // Đảm bảo rằng userId được gán đúng giá trị
        $.ajax({
            url: '/Notifications/GetUserNotifications',
            method: 'GET',
            data: { userId: userId },
            success: function (data) {
                $('#notificationList').empty();
                data.forEach(function (notification) {
                    console.log(notification);
                    var notificationHtml = '<p class="';
                    notificationHtml += notification.isRead ? 'read-notification' : 'unread-notification';
                    notificationHtml += '">' + notification.notification_text + ' - ' + new Date(notification.createAt).toLocaleString() + '</p>';
                    $('#notificationList').append(notificationHtml);

                    // Add click event to mark notification as read
                    $('#notificationList').on('click', 'p', function () {
                        if (!notification.isRead) {
                            markNotificationAsRead(notification.notification_id);
                            $(this).removeClass('unread-notification').addClass('read-notification');
                        }
                    });
                });
            },
            error: function () {
                $('#notificationList').append('<p>Failed to load notifications.</p>');
            }
        });
    });
});

// Function to mark notification as read
function markNotificationAsRead(notification_id) {
    console.log("notificationId is: " + notification_id);
    $.ajax({
        url: '/Notifications/MarkAsRead',
        method: 'POST',
        data: { notificationId: notification_id },
        success: function (response) {
            console.log('Notification marked as read.');
        },
        error: function (xhr, status, error) {
            console.error('Failed to mark notification as read.');
            console.error(xhr.responseText);
            console.error(status);
            console.error(error);
        }
    });
}