(function ($) {
    var previousEditUserFactory = abp.modals.editUser;

    abp.modals.editUser = function () {
        var previous = previousEditUserFactory ? previousEditUserFactory() : null;

        return {
            initModal: function (publicApi, args) {
                if (previous && previous.initModal) {
                    previous.initModal(publicApi, args);
                }

                var $modal = publicApi.getModal();
                var $container = $modal.find('#organization-unit-cascade');
                var initialPath = [];

                try {
                    initialPath = JSON.parse($container.attr('data-initial-path') || '[]');
                } catch (e) {
                    initialPath = [];
                }

                internalRequestsManagement.organizationUnitCascade.init($container, initialPath);
            }
        };
    };
})(jQuery);
