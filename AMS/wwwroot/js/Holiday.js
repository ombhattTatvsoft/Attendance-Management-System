$(document).ready(function () {
    var calendar = new FullCalendar.Calendar(document.getElementById('holidayCalendar'), {
      headerToolbar: {
        left: 'prev,next today',
        center: 'title',
        right: 'dayGridMonth,listYear'
      },
      initialView: 'dayGridMonth',
      events: '/Holiday/GetHolidays',
      dateClick: function (info) {
        $('#holidayForm')[0].reset();
        $('#btnDelete').hide();
        $('#holidayModal').modal('show');
        $('input[name="HolidayId"]').val(0);
        $('input[name="HolidayDate"]').val(info.dateStr);
      },
      eventClick: function (info) {
        $('#btnDelete').show();
        $.ajax({
          url: '/Holiday/GetHoliday',
          type: 'GET',
          data: { id: info.event.id },
          success: function (response) {
            if (response.success) {
              $('#HolidayId').val(response.data.holidayId);
              $('input[name="HolidayName"]').val(response.data.holidayName);
              $('input[name="HolidayDate"]').val(response.data.holidayDate);
              $('#holidayModal').modal('show');
            }
            else {
              toastr.error(response.message);
            }
          },
        });
      }
    });
    calendar.render();

    $('#holidayForm').on('submit', function (e) {
      e.preventDefault();
      var formData = $(this).serialize();
      $.ajax({
        url: '/Holiday/SaveHoliday',
        type: 'POST',
        data: formData,
        success: function (response) {
          if (response.success) {
            toastr.success(response.message);
            $('#holidayModal').modal('hide');
            calendar.refetchEvents();
          } else {
            toastr.error(response.message);
          }
        },
      });
    });

    $('#btnDelete').on('click', function () {
      var holidayId = $('#HolidayId').val();
      if (holidayId) {
        $.ajax({
          url: '/Holiday/DeleteHoliday',
          type: 'POST',
          data: { id: holidayId },
          success: function (response) {
            if (response.success) {
              toastr.success(response.message);
              $('#holidayModal').modal('hide');
              calendar.refetchEvents();
            } else {
              toastr.error(response.message);
            }
          }
        });
      }
    });
  });