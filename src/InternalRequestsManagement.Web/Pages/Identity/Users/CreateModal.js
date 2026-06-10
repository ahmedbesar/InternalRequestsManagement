(function ($) {
    var previousCreateUserFactory = abp.modals.createUser;

    abp.modals.createUser = function () {
        var previous = previousCreateUserFactory ? previousCreateUserFactory() : null;

        return {
            initModal: function (publicApi, args) {
                if (previous && previous.initModal) {
                    previous.initModal(publicApi, args);
                }

                var $modal = publicApi.getModal();
                internalRequestsManagement.organizationUnitCascade.init(
                    $modal.find('#organization-unit-cascade'));
            }
        };
    };
})(jQuery);
