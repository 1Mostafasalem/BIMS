var table;
var datatable;
var updatedRow;
var exportedCols = [];

function ShowMessage(message = 'Saved Success', IsSuccess = true) {
    if (message !== '') {
        Swal.fire({
            icon: (IsSuccess === true ? 'success' : 'error'),
            title: (IsSuccess === true ? 'Success' : 'Error'),
            text: message,
        })
    }
}

function showSuccessMessage(message = 'Saved successfully!') {
    Swal.fire({
        icon: 'success',
        title: 'Success',
        text: message,
        customClass: {
            confirmButton: "btn btn-primary "
        }
    });
}

function showErrorMessage(message = 'Something went wrong!') {
    Swal.fire({
        icon: 'error',
        title: 'Oops...',
        text: message.responseText != undefined ? message.responseText : message,
        customClass: {
            confirmButton: "btn btn-primary "
        }
    });
}

function disableSubmitButton(btn) {
    //$('body :submit').attr('disabled', 'disabled').attr('data-kt-indicator', 'on');
    $(btn).attr('disabled', 'disabled').attr('data-kt-indicator', 'on');
}

function onModalBegin() {
    debugger;

    disableSubmitButton($('#Modal').find(':submit'));
}

function onModalSuccess(row) {
    debugger;
    showSuccessMessage();
    $("#Modal").modal('hide');
    if (updatedRow !== undefined) {
        datatable.row(updatedRow).remove().draw();
    }
    var newRow = $(row);
    datatable.row.add(newRow).draw();

    KTMenu.init();
    KTMenu.initHandlers();
}

function onModalComplete(row) {
    //$('body :submit').removeAttr('disabled', 'disabled');
    //$('body :submit span')[1].innerText = 'Save';
    //$("#SaveButtonSpinner").addClass('d-none');
    $('body :submit').removeAttr('disabled').removeAttr('data-kt-indicator');
}

function ApplySelect2() {
    $('.js-select2').select2({
        //width: 'resolve' // need to override the changed default
    });
    $('.js-select2').on("change", function (e) {
        var select = $(this);
        $('form').not('#SignOut').validate().element("#" + select.attr('id'));
    });
}

$(document).ready(function () {
    var message = $('#Message').text();
    ShowMessage(message);


    //Disable Submit Button
    $('form').not('#SignOut').on('submit', function () {
        debugger;
        if ($('.js-tinymce').length > 0) {
            $('.js-tinymce').each(function () {
                debugger;
                var input = $(this);
                var content = tinymce.get(input.attr('id')).getContent();
                input.val(content);
            });
        }

        var isVaild = $(this).valid();
        if (isVaild) {
            debugger;
            disableSubmitButton($(this).find(':submit'));
        }
    });

    //DataTable
    UIDatatables.init();

    //Select2
    ApplySelect2();

    //Flat Picker
    //$('.js-flatpickr').flatpickr({
    //    dateFormat: "d/m/Y"
    //});

    //Datepicker
    $('.js-datepicker').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'up',
        maxDate: new Date()
    });

    //TinyMCE
    if ($('.js-tinymce').length > 0) {
        var options = { selector: ".js-tinymce", height: "430" };

        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }

        tinymce.init(options);
    }

    //Handel Bootstrap Modal
    $('body').delegate('.js-render-modal', 'click', function () {
        debugger;
        var btn = $(this);
        var modal = $('#Modal');
        modal.find('#ModalLabel').text(btn.data('title'));
        if (btn.data('update') !== undefined) {
            updatedRow = btn.parents('tr');
        }

        $.get({
            url: btn.data('url'),
            success: function (form) {
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(modal);
                ApplySelect2();
            },
            Error: function () {
                showErrorMessage(error);
            },
        });
        modal.modal('show');

    });

    //Handel Toggle Status
    $('body').delegate('.js-toggle-status', 'click', function () {
        var btn = $(this);
        bootbox.confirm({
            message: "Are You Sure That You Need Toggle?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {

                    var url = btn.data('url');
                    $.post({
                        url: url,
                        data: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                        success: function (lastUpdatedOnValue) {
                            debugger;
                            var row = btn.parents('tr');
                            var status = row.find('.js-status');
                            var newStatus = status.text().trim() === 'Deleted' ? 'Available' : 'Deleted';
                            status.text(newStatus).toggleClass('badge-light-success badge-light-danger');
                            var lastUpdatedOn = row.find('.js-updated-on');
                            lastUpdatedOn.html(lastUpdatedOnValue);
                            row.addClass('animate__animated animate__flash');
                            ShowMessage('Saved Successfully');
                        },
                        error: function () {
                            showErrorMessage('Something went wrong!');
                        },
                    });
                }
            }
        });
    });

    //Handel confirm
    $('body').delegate('.js-confirm', 'click', function () {
        var btn = $(this);
        bootbox.confirm({
            message: btn.data('message'),
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success '
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {

                    var url = btn.data('url');
                    $.post({
                        url: url,
                        data: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                        success: function () {
                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        },
                    });
                }
            }
        });
    });

    // Handel Signout
    $('.js-signout').on('click', function () {
        debugger;
        $('#SignOut').submit();
    });

});

//DataTables
var headers = $('th');
$.each(headers, function (i) {
    if (!$(this).hasClass('js-no-export'))
        exportedCols.push(i);
});
// Class definition 
var UIDatatables = function () {
    // Private functions
    var initDatatable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
            'drawCallback': function () {
                KTMenu.createInstances();
            },
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-datatables').data('document-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();
                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = $('.js-datatables');
            if (!$(table).length) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();


//window.addEventListener('load', function () {
//    $("#Main-spinner").addClass('d-none');
//})