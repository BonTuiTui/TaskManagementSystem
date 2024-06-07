//$(document).ready(function () {
//    // Gọi AJAX để lấy thông tin notifications của userId
//    $('#notificationLink').on('click', function () {

//        var userId = '@userId';
//        console.log("site js is " + userId);
//        $.ajax({
//            url: '/Notifications/GetUserNotifications',
//            method: 'GET',
//            data: { userId: userId }, // Truyền userId qua query string
//            success: function (data) {
//                $('#notificationList').empty();
//                data.forEach(function (notification) {
//                    $('#notificationList').append('<p>' + notification.message + ' - ' + new Date(notification.createAt).toLocaleString() + '</p>');
//                });
//            },
//            error: function () {
//                $('#notificationList').append('<p>Failed to load notifications.</p>');
//            }
//        });
//    });
//});
