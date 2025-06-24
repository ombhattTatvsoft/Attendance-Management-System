const today = new Date().toISOString().split('T')[0];
$(document).ready(function () {
    $("#startDate,#endDate").prop('max', today);
    $("#endDate").val(today);
    $('#startDate').change(function () {
        $("#endDate").prop('min', $(this).val());
    });
    $('#endDate').change(function () {
        $("#startDate").prop('max', $(this).val());
    });
    fetchReport();
});
function resetReport() {
    $("#startDate").val("");
    $("#endDate").val(today);
    $("#startDate,#endDate").removeAttr('min');
    $("#startDate,#endDate").prop('max', today);
    fetchReport();
}
function fetchReport() {
    $.ajax({
        url: '/ReportAnalytics/FetchReport',
        type: 'POST',
        data: {
            startDate: $("#startDate").val(),
            endDate: $("#endDate").val()
        },
        success: function (response) {
            $("#reportTableContainer").html(response);
            ReportDataTable();
        }
    });
}
function ReportDataTable() {
    var reportTable = $('#reportTable').DataTable({
        responsive: true,
        pageLength: 10,
        lengthMenu: [5, 10, 25, -1],
        scrollCollapse: true,
        scrollY: '415px',
        "order": [[4, "desc"]],
        "columnDefs": [
            { "orderable": false, "targets": [0, 1, 2, 3] }
        ],
        layout: {
            bottomStart: null,
            topStart: {
                buttons: [
                    {
                        extend: 'excel',
                        text: 'Export to Excel',
                        className: 'sitebgcolor rounded text-white',
                    }, {
                        extend: 'pdf',
                        text: 'Export to PDF',
                        className: 'border border-dark rounded bg-white',
                    }
                ]
            },
            bottomEnd: [
                {
                    info: {
                        text: "Showing _START_ - _END_ of _TOTAL_"
                    }

                },
                {
                    pageLength: {
                        text: "_MENU_",
                    }
                },
                {
                    paging: {
                        buttons: 3
                    }
                }
            ]
        }
    });
}