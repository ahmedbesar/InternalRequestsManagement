(function ($) {
    var l = abp.localization.getResource('InternalRequestsManagement');

    var requestService = internalRequestsManagement.requests.request;
    var currentScope = 0;
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'Requests/CreateModal',
        modalClass: 'createRequest'
    });
    var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'Requests/EditModal',
        modalClass: 'editRequest'
    });
    var detailModal = new abp.ModalManager(abp.appPath + 'Requests/DetailModal');
    var changeStatusModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'Requests/ChangeStatusModal',
        modalClass: 'changeRequestStatus'
    });
    var assignModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'Requests/AssignModal',
        modalClass: 'assignRequest'
    });

    var dataTable = $('#RequestsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(requestService.getList, function () {
                return {
                    search: $('#SearchInput').val(),
                    status: $('#StatusFilter').val() !== '' ? parseInt($('#StatusFilter').val()) : null,
                    priority: $('#PriorityFilter').val() !== '' ? parseInt($('#PriorityFilter').val()) : null,
                    requestTypeId: $('#TypeFilter').val() || null,
                    scope: currentScope
                };
            }),
            columnDefs: [
                {
                    title: l('Actions'),
                    orderable: false,
                    rowAction: {
                        items: [
                            {
                                text: l('View'),
                                action: function (data) {
                                    detailModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('InternalRequestsManagement.Requests.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('ChangeStatus'),
                                visible: abp.auth.isGranted('InternalRequestsManagement.Requests.ChangeStatus'),
                                action: function (data) {
                                    changeStatusModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('AssignRequest'),
                                visible: abp.auth.isGranted('InternalRequestsManagement.Requests.Assign'),
                                action: function (data) {
                                    assignModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('InternalRequestsManagement.Requests.Delete'),
                                confirmMessage: function () { return l('AreYouSure'); },
                                action: function (data) {
                                    requestService.delete(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l('SuccessfullyDeleted'));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                    }
                },
                {
                    title: l('RequestTitle'),
                    data: 'title',
                    render: function (data, type, row) {
                        var badges = '';
                        if (row.isOverdue) {
                            badges += '<span class="badge bg-danger ms-1">' + l('Overdue') + '</span>';
                        }
                        if (!row.assignedUserId) {
                            badges += '<span class="badge bg-warning ms-1">' + l('Unassigned') + '</span>';
                        }
                        return $('<div>').text(data).html() + badges;
                    }
                },
                {
                    title: l('RequestType'),
                    data: 'requestTypeName',
                    orderable: false
                },
                {
                    title: l('RequestPriority'),
                    data: 'priority',
                    render: function (data) {
                        var map = { 1: 'Low', 2: 'Normal', 3: 'High', 4: 'Critical' };
                        var cls = { 1: 'secondary', 2: 'primary', 3: 'warning', 4: 'danger' };
                        return '<span class="badge bg-' + (cls[data] || 'secondary') + '">' + l('Priority:' + (map[data] || data)) + '</span>';
                    }
                },
                {
                    title: l('RequestStatus'),
                    data: 'status',
                    orderable: false,
                    render: function (data) {
                        var map = { 1: 'Draft', 2: 'Submitted', 3: 'InProgress', 4: 'OnHold', 5: 'Resolved', 6: 'Closed', 7: 'Cancelled', 8: 'Rejected' };
                        var cls = { 1: 'secondary', 2: 'info', 3: 'primary', 4: 'warning', 5: 'success', 6: 'dark', 7: 'secondary', 8: 'danger' };
                        return '<span class="badge bg-' + (cls[data] || 'secondary') + '">' + l('Status:' + (map[data] || data)) + '</span>';
                    }
                },
                {
                    title: l('Requester'),
                    data: 'requesterName',
                    orderable: false
                },
                {
                    title: l('AssignedUser'),
                    data: 'assignedUserName',
                    orderable: false,
                    defaultContent: '<em class="text-muted">—</em>'
                },
                {
                    title: l('OrganizationUnit'),
                    data: 'organizationUnitName',
                    orderable: false
                },
                {
                    title: l('DueDate'),
                    data: 'dueDate',
                    orderable: false,
                    render: function (data, type, row) {
                        if (!data) return '<em class="text-muted">—</em>';
                        var months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
                        var d = new Date(data);
                        var str = d.getDate() + ' ' + months[d.getMonth()] + ' ' + d.getFullYear();
                        if (row.isOverdue) {
                            str = '<span class="text-danger fw-bold">' + str + '</span>';
                        }
                        return str;
                    }
                }
            ]
        })
    );

    $('#scopeTabs').on('click', 'a', function (e) {
        e.preventDefault();
        $('#scopeTabs .nav-link').removeClass('active');
        $(this).addClass('active');
        currentScope = parseInt($(this).data('scope'));
        dataTable.ajax.reload();
    });

    var filterTimer;
    $('#SearchInput,#StatusFilter,#PriorityFilter,#TypeFilter').on('change keyup', function () {
        clearTimeout(filterTimer);
        filterTimer = setTimeout(function () {
            dataTable.ajax.reload();
        }, 400);
    });

    $('#NewRequestButton').click(function () {
        createModal.open();
    });

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    changeStatusModal.onResult(function () {
        dataTable.ajax.reload();
    });

    assignModal.onResult(function () {
        dataTable.ajax.reload();
    });

})(jQuery);
