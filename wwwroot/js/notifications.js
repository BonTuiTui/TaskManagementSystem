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
                    var notificationHtml = '<p class="notification-item ';
                    notificationHtml += notification.isRead ? 'read-notification' : 'unread-notification';
                    notificationHtml += '" data-id="' + notification.notification_id + '">' + notification.notification_text + ' - ' + new Date(notification.createAt).toLocaleString() + '</p>';
                    $('#notificationList').append(notificationHtml);
                });

                // Add click event to each notification item
                $('.notification-item').on('click', function () {
                    var notificationId = $(this).data('id');
                    if (!$(this).hasClass('read-notification')) {
                        markNotificationAsRead(notificationId);
                        $(this).removeClass('unread-notification').addClass('read-notification');
                    }
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