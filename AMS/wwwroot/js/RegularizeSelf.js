$(document).ready(function(){
    const today = new Date().toISOString().split('T')[0];
    $("#attendanceDate").prop('max', today);
    $("#attendanceDate").prop('min', createdAt);

    $("#attendanceDate").change(function(){
        $.ajax({
            type: "GET",
            url: "/Attendance/GetAttendanceByDateAndUser",
            data: {date:$(this).val(),userId:userId},
            success: function (response) {
                $("#submittedAttendance").html(`<option value="${response.data}">${response.data}</option>`);
                const select=document.getElementById('correctAttendance');
                for(let i=0;i<select.options.length;i++){
                    if(select.options[i].value==response.data){
                        select.remove(i);
                        break;
                    }
                }
            }
        });
    }); 

    $('#upsertRegularize').on('submit', function (e) {
        e.preventDefault();
        var formData = $(this).serialize();
        
        $.ajax({
          url: '/Attendance/UpsertRegularize',
          type: 'POST',
          data: formData,
          success: function (response) {
            if (response.success) {
              toastr.success(response.message);
            } else {
              toastr.error(response.message);
            }
          }
        });
      });
});