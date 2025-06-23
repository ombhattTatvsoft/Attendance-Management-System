const currentMonth = new Date().getMonth() + 1;
var currentElement;
var newAttendance = [];
$(document).ready(function () {
    $('#userSearch').on('keyup', function () {
        var value = $(this).val().toLowerCase();
        $("#teamAttendanceBody tr").hide();
        $("#teamAttendanceBody tr .user-name").each(function(){
            var userName = $(this).text().toLowerCase();
            if (userName.indexOf(value) > -1) {
                $(this).closest('tr').show().next('tr').show();
            }
        })
    });
    $("#teamMonth").val(currentMonth);
    if (teamUsers.length > 0) {
        getTeamAttendanceTable();
    }
    $(document).on('change', '.attendance-table-wrapper select', function () {
        $(currentElement).attr('data-status', $(this).val());
        $(this).removeClass('today-not-filled');
    });
});
function getTeamAttendanceTable() {
    $("#teamAttendanceHeaderRow").empty();
    $("#teamAttendanceBody").empty();
    var flag=0;
    teamUsers.forEach((element, index) => {
        $.ajax({
            type: "GET",
            async: false,
            url: "/Attendance/GetAttendanceTable",
            data: {
                month: $("#teamMonth").val(),
                year: $("#teamYear").val(),
                userId: element.id
            },
            success: function (data) {
                if (data.success === false) {
                    toastr.error("Failed loading attendance table");
                } else {
                    let wrapper = $('<div></div>').html(data);
                    $("#teamAttendanceTable").before(wrapper.find('#approvedTitle'));
                    if (index === 0) {
                        let header = wrapper.find("thead > tr").html();
                        $("#teamAttendanceHeaderRow").html(`<th>Name</th>${header}`);
                    }

                    let userRow = wrapper.find("tbody > tr").html();
                    let userName = element.name;
                    let summaryHtml = wrapper.find("h6").eq(1).html();

                    let attendanceRow = `<tr><td class="text-nowrap user-name"><strong>${userName}</strong></td>${userRow}</tr>`;
                    let summaryRow = `<tr><td></td><td colspan="${wrapper.find('thead > tr > th').length}">${summaryHtml}</td></tr>`;
                    $("#teamAttendanceBody").append(attendanceRow).append(summaryRow);
                    if(flag==0){
                        if (wrapper.find('td').hasClass('clickable')) {
                            flag=1;
                        }
                    }
                }
            }
        });
        if(flag==1){
            $("#saveTeamAttendanceBtn").removeClass('disabled');
            $("#approvedTitle").addClass('d-none');
        }
        else{
            $("#saveTeamAttendanceBtn").addClass('disabled');
            if ($("#myMonth").val() == currentMonth)
                $("#approvedTitle").removeClass('d-none');
        }
    });
}
function resetTeamAttendanceTable() {
    $("#teamMonth").val(currentMonth);
    getTeamAttendanceTable();
}
function showDropdown(ele) {
    $(ele).attr('data-status', $(ele).find('select').val());
    currentElement = ele;
}
function saveTeamAttendance() {
    var oldNotApproved = $("#teamAttendanceTable select").filter(function () { return $(this).val() != "" && !$(this).hasClass('approved') });
    var newUpdated = $("#teamAttendanceTable [data-status]");
    newAttendance = [];
    $(oldNotApproved).each(function () {
        newAttendance.push({
            UserId: $(this).closest('td').data('userid'),
            Date: $(this).closest('td').data('date'),
            Status: $(this).val()
        });
    });
    $(newUpdated).each(function () {
        newData = {
            UserId: $(this).closest('td').data('userid'),
            Date: $(this).closest('td').data('date'),
            Status: $(this).closest('td').data('status')
        }
        if(!newAttendance.some(record => record.UserId === newData.UserId && record.Date === newData.Date))
            newAttendance.push(newData);
    });
    if (newAttendance.length > 0) {
        var formData = {
            UserId: currentUserId,
            Month: parseInt($("#teamMonth").val()),
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
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.isSuccess) {
                    toastr.success(response.message)
                    getTeamAttendanceTable();
                    $("#userSearch").trigger('keyup');
                }
                else
                    toastr.error(response.message)
            }
        });
    }
}