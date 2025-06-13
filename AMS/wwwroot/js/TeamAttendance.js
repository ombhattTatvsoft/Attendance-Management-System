const currentMonth = new Date().getMonth() + 1;
var currentElement;
var newAttendance=[];
$(document).ready(function () {
    $("#teamMonth").val(currentMonth);
    if(teamUsers.length>0){ 
        getTeamAttendanceTable();
    }
    $(document).on('change','.attendance-table-wrapper select', function () {
        $(currentElement).attr('data-status', $(this).val());
        $(this).removeClass('today-not-filled');
    });
});
function getTeamAttendanceTable(){
    $("#teamAttendanceTable").empty();
    teamUsers.forEach(element => {
        $.ajax({
            type: "GET",
            url: "/Attendance/GetAttendanceTable",
            data: {
                month: $("#teamMonth").val(),
                year: $("#teamYear").val(),
                userId:element.id
            },
            success: function (data) {
                if (data.success == false)
                    toastr.error("Failed loading attendance table")
                else{
                    let wrapper=$('<div></div>').html(data);
                    wrapper.find('.user-title').text(element.name);
                    $("#teamAttendanceTable").append(wrapper.html()); 
                    if(!$("#teamAttendanceTable td").hasClass('clickable')){
                        $("#saveTeamAttendanceBtn").hide()
                    }
                    else{
                        $("#saveTeamAttendanceBtn").show()
                    }
                }
            }
        });
    });
}
function resetTeamAttendanceTable() {
    $("#teamMonth").val(currentMonth);
    getTeamAttendanceTable();
}
function showDropdown(ele){
    $(ele).attr('data-status', $(ele).find('select').val());
    currentElement=ele;
}
function saveTeamAttendance(){
    var toBeChanged=$("#teamAttendanceTable select").filter(function() {return $(this).val()!=""});
    newAttendance = [];
    $(toBeChanged).each(function(){
        newAttendance.push({
            UserId:$(this).closest('td').data('userid'),
            Date:$(this).closest('td').data('date'),
            Status:$(this).val()
        });
    });
    if(newAttendance.length>0){
        var formData={
            UserId:currentUserId,
            Month:parseInt($("#teamMonth").val()),
            Year: parseInt($("#teamYear").val()),
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
                    getTeamAttendanceTable();
                }
                else
                    toastr.error(response.message)
            }
        });
    }
}