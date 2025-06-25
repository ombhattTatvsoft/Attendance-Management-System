const currentMonth = new Date().getMonth() + 1;
var currentElement;
var newAttendance=[];
$(document).ready(function () {
    $("#myMonth").val(currentMonth);
    getMyAttendanceTable(currentUserId);
    $(document).on('change','.attendance-table-wrapper select', function () {
        $(currentElement).attr('data-status', $(this).val());
        $(this).removeClass('today-not-filled');
    });
});
function getMyAttendanceTable(id) {
    $.ajax({
        type: "GET",
        url: "/Attendance/GetAttendanceTable",
        data: {
            month: $("#myMonth").val(),
            year: $("#myYear").val(),
            userId:id
        },
        success: function (data) {
            if (data.success == false)
                toastr.error("Failed loading attendance table")
            else{
                $("#myAttendanceTable").html(data);
                if(!$("#myAttendanceTable td").hasClass('clickable')){
                    $("#saveMyAttendanceBtn").addClass('disabled');
                    if($("#myMonth").val()==currentMonth)
                    $("#approvedTitle").removeClass('d-none');
                }
            }
        }
    });
}
function resetMyAttendanceTable(id) {
    $("#myMonth").val(currentMonth);
    getMyAttendanceTable(id);
}
function showDropdown(ele){
    $(ele).attr('data-status', $(ele).find('select ').val());
    currentElement=ele;
}
function saveMyAttendance(){
    newAttendance = [];
    $("#myAttendanceTable [data-status]").each(function(){
        newAttendance.push({
            UserId:$(this).data('userid'),
            Date:$(this).data('date'),
            StatusId:$(this).data('status')
        });
    });
    console.log(newAttendance);
    if(newAttendance.length>0){
        var formData={
            UserId:newAttendance[0].UserId,
            Month:parseInt($("#myMonth").val()),
            Year: parseInt($("#myYear").val()),
            AttendanceRecords: newAttendance.map(record => ({
                UserId: record.UserId,
                Date: record.Date,
                StatusId: record.StatusId
            }))
        }
        console.log(formData);
        $.ajax({
            type: "POST",
            url: "/Attendance/SaveAttendances",
            contentType:'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if(response.isSuccess){
                    toastr.success(response.message)
                    getMyAttendanceTable(currentUserId);
                }
                else
                    toastr.error(response.message)
            }
        });
    }
}