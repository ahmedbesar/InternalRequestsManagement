(function ($) {
    var l = abp.localization.getResource('InternalRequestsManagement');
    var requestService = internalRequestsManagement.requests.request;

    var noteRequiredStatuses = [4, 7, 8]; // OnHold=4, Cancelled=7, Rejected=8

    abp.modals.changeRequestStatus = function () {
        return {
            initModal: function (publicApi, args) {
                var $modal = publicApi.getModal();

                $modal.find('#NewStatusSelect').on('change', function () {
                    var status = parseInt($(this).val());
                    var needsNote = noteRequiredStatuses.indexOf(status) >= 0;
                    $modal.find('#NoteRequired').toggle(needsNote);
                    $modal.find('#NoteHint').toggle(needsNote);
                    $modal.find('#StatusNote').prop('required', needsNote);
                });

                $modal.find('#ChangeStatusForm').on('submit', function (e) {
                    e.preventDefault();
                    var id = $modal.find('#StatusRequestId').val();
                    var input = {
                        newStatus: parseInt($modal.find('#NewStatusSelect').val()),
                        note: $modal.find('#StatusNote').val() || null
                    };

                    requestService.changeStatus(id, input).then(function (result) {
                        if (!result.isSuccess) {
                            $modal.find('#StatusResultError').text(result.errors[0]?.message).show();
                            return;
                        }
                        abp.notify.success(l('SuccessfullySaved'));
                        publicApi.close();
                        window.location.reload();
                    });
                });
            }
        };
    };

})(jQuery);
