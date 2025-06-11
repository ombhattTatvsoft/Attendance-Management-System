$(document).ready(function () {
    var currentMonth=new Date().getMonth()+1;
    $("#month").val(currentMonth);
    getAttendanceTable();    
});
function getAttendanceTable(){
    $.ajax({
        type: "GET",
        url: "/Attendance/GetAttendanceTable",
        data: {
            month: $("#month").val(),
            year: $("#year").val()
        },
        success: function (data) {
            if(data.success==false)
                toastr.error("Failed loading attendance table")
            else
                $("#attendanceTable").html(data);
        }
    });
}
function ResetAttendanceTable(){
    $("#month").val(currentMonth);
    getAttendanceTable();
}