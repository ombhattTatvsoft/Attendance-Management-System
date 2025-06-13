const currentMonth = new Date().getMonth() + 1;
var currentElement;
var newAttendance=[];
$(document).ready(function () {
    if(currentRoleId!=1){
        $("#month").val(currentMonth);
        getAttendanceTable(currentUserId);
    }
    $(document).on('change','.attendance-table-wrapper select', function () {
        $(currentElement).attr('data-status', $(this).val());
        $(this).removeClass('today-not-filled');
    });
});
function getAttendanceTable(id) {
    $.ajax({
        type: "GET",
        url: "/Attendance/GetAttendanceTable",
        data: {
            month: $("#month").val(),
            year: $("#year").val(),
            userId:id
        },
        success: function (data) {
            if (data.success == false)
                toastr.error("Failed loading attendance table")
            else
                $("#attendanceTable").html(data);
        }
    });
}
function ResetAttendanceTable(id) {
    $("#month").val(currentMonth);
    getAttendanceTable(id);
}
function showDropdown(ele){
    $(".attendance-table-wrapper select").removeClass('d-none');
    $(ele).attr('data-status', $('.attendance-table-wrapper select').val());
    currentElement=ele;
}
function saveAttendance(){
    newAttendance = [];
    $("[data-status]").each(function(){
        newAttendance.push({
            UserId:$(this).data('userid'),
            Date:$(this).data('date'),
            Status:$(this).data('status')
        });
    });
    if(newAttendance.length>0){
        var formData={
            UserId:newAttendance[0].UserId,
            Month:parseInt($("#month").val()),
            Year: parseInt($("#year").val()),
            AttendanceRecords: newAttendance.map(record => ({
                UserId: record.UserId,
                Date: record.Date,
                Status: record.Status
            }))
        }
        $.ajax({
            type: "POST",
            url: "/Attendance/SaveAttendances",
            contentType:'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if(response.isSuccess){
                    toastr.success(response.message)
                    getAttendanceTable(currentUserId);
                }
                else
                    toastr.error(response.message)
            }
        });
    }
}
