(function ($) {
    var l = abp.localization.getResource('InternalRequestsManagement');
    var dashboardService = internalRequestsManagement.requests.requestDashboard;

    var statusLabels = {
        1: l('Status:Draft'), 2: l('Status:Submitted'), 3: l('Status:InProgress'),
        4: l('Status:OnHold'), 5: l('Status:Resolved'), 6: l('Status:Closed'),
        7: l('Status:Cancelled'), 8: l('Status:Rejected')
    };

    var statusColors = {
        1: '#6c757d', 2: '#0dcaf0', 3: '#0d6efd', 4: '#ffc107',
        5: '#198754', 6: '#343a40', 7: '#adb5bd', 8: '#dc3545'
    };

    dashboardService.get().then(function (data) {
        $('#OpenCount').text(data.openCount);
        $('#OverdueCount').text(data.overdueCount);
        $('#UnassignedCount').text(data.unassignedCount);

        renderStatusChart(data.byStatus);
        renderTypeChart(data.byType);
        renderOuChart(data.byOrganizationUnit);
        renderAssigneesTable(data.topAssignees);
    });

    function renderStatusChart(data) {
        new Chart(document.getElementById('StatusChart'), {
            type: 'doughnut',
            data: {
                labels: data.map(function (d) { return statusLabels[d.status] || d.status; }),
                datasets: [{
                    data: data.map(function (d) { return d.count; }),
                    backgroundColor: data.map(function (d) { return statusColors[d.status] || '#999'; })
                }]
            },
            options: { plugins: { legend: { position: 'right' } } }
        });
    }

    function renderTypeChart(data) {
        new Chart(document.getElementById('TypeChart'), {
            type: 'bar',
            data: {
                labels: data.map(function (d) { return d.requestTypeName; }),
                datasets: [{
                    label: l('RequestCount'),
                    data: data.map(function (d) { return d.count; }),
                    backgroundColor: '#0d6efd'
                }]
            },
            options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
        });
    }

    function renderOuChart(data) {
        new Chart(document.getElementById('OuChart'), {
            type: 'bar',
            data: {
                labels: data.map(function (d) { return d.organizationUnitName; }),
                datasets: [{
                    label: l('RequestCount'),
                    data: data.map(function (d) { return d.count; }),
                    backgroundColor: '#198754'
                }]
            },
            options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
        });
    }

    function renderAssigneesTable(data) {
        var html = '';
        if (!data || data.length === 0) {
            html = '<tr><td colspan="2" class="text-muted text-center">—</td></tr>';
        } else {
            $.each(data, function (_, d) {
                html += '<tr><td>' + abp.utils.htmlEncode(d.userName) + '</td><td class="text-end"><span class="badge bg-primary">' + d.count + '</span></td></tr>';
            });
        }
        $('#AssigneesBody').html(html);
    }

})(jQuery);
