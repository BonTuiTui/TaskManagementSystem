// Function to mark notification as read and update unread count
function markNotificationAsRead(notificationId) {
    console.log("notificationId is: " + notificationId);
    $.ajax({
        url: '/Notifications/MarkAsRead',
        method: 'POST',
        data: { notificationId: notificationId },
        success: function (response) {
            console.log('Notification marked as read.');
            // After marking as read, update the unread notifications count
            updateUnreadNotificationsCount();
        },
        error: function (xhr, status, error) {
            console.error('Failed to mark notification as read.');
            console.error(xhr.responseText);
            console.error(status);
            console.error(error);
        }
    });
}

// Function to update unread notifications count
function updateUnreadNotificationsCount() {
    var userId = window.userId; // Ensure userId is correctly set
    if (userId) {
        $.ajax({
            url: '/Notifications/GetUnreadNotificationsCount',
            method: 'GET',
            data: { userId: userId },
            success: function (count) {
                if (count > 0) {
                    $('#unreadCountBadge').text(count).show();
                } else {
                    $('#unreadCountBadge').hide();
                }
            },
            error: function () {
                console.error('Failed to load unread notifications count.');
            }
        });
    }
}

$(document).ready(function () {
    // Load the unread notifications count on page load
    updateUnreadNotificationsCount();

    // Load notifications when the modal is opened
    $('#notificationLink').on('click', function () {
        var userId = window.userId; // Ensure userId is correctly set
        if (userId) {
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
        }
    });
});