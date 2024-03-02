
var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inProcess")) {
        loadDataTable("inProcess")
    }
    else {
        if (url.includes("pending")) {
            loadDataTable("pending")
        }
        else {

        }
        if (url.includes("completed")) {
            loadDataTable("completed")
        }
        else {
            loadDataTable("all")
        }

    }
});
function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Order/GetAll?status='+status },
        "columns": [
            { data: 'id', "width": "15%" },
            { data: 'name', "width": "15%" },
            { data: 'applicationUser.email', "width": "10%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'taotalOrder', "width": "10%"},
            {
                data: 'id',
                "render": function (data) {
                    return `<div class=" w-75 btn-group" role="group">
                            <a href="/order/details?orderId=${data}" class="btn btn-primary mx-2 " ><i class="bi bi-pencil-square"></i>  </a> 
                            </div > ` 
                },
                "width":"15%"
            }
        ]
    });
}
