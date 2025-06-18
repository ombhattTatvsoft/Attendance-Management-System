const currentMonth = new Date().getMonth() + 1;
var data;
$(document).ready(function () {
    $("#myMonth").val(currentMonth);
    // Load the Visualization API and the corechart package.
    google.charts.load('current', { 'packages': ['corechart'] });
    // Set a callback to run when the Google Visualization API is loaded.
    google.charts.setOnLoadCallback(fetchGraph);
});
function fetchGraph() {
    $.ajax({
        type: "GET",
        url: "/Dashboard/GetChartData",
        data: {
            month: $("#myMonth").val(),
            year: $("#myYear").val(),
            userId: $("#selectedUserId").val()
        },
        success: function (response) {
            drawChart(response);
        }
    });
}

function drawChart(dataArray) {
    // Create the data table.
    data = new google.visualization.DataTable();
    data.addColumn('string', 'Status');
    data.addColumn('number', 'Count');
    // Add the data from the array
    if (dataArray.remainingDays > 0) {
        data.addRows([
            ['Present', dataArray.presentDays],
            ['Absent', dataArray.fullAbsentDays],
            ['Half Days', dataArray.halfDays],
            ['Remaining Days', dataArray.remainingDays]
        ]);
    }

    // Set chart options
    var options = {
        title: 'Attendance Overview',
        colors: ['green', 'red', 'rgb(56, 162, 197)', '#f0ad4e'],
        pieHole: 0.4
    };

    // Instantiate and draw our chart, passing in some options.
    var chart = new google.visualization.PieChart(document.getElementById('chart_div'));
    chart.draw(data, options);
}