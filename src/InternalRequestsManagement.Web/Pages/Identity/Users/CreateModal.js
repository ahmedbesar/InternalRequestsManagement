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
                initOrganizationUnitCascade($modal.find('#organization-unit-cascade'));
            }
        };
    };

    function initOrganizationUnitCascade($container) {
        if (!$container.length) {
            return;
        }

        $container.off('change.ouCascade').on('change.ouCascade', '.ou-level-select', function () {
            var $select = $(this);
            var $levelContainer = $select.closest('.ou-level-container');
            var selectedId = $select.val();
            var $hiddenInput = $container.find('#OrganizationUnitId');

            $levelContainer.nextAll('.ou-level-container').remove();

            if (!selectedId) {
                var $previousSelect = $levelContainer.prev('.ou-level-container').find('.ou-level-select');
                $hiddenInput.val($previousSelect.length ? $previousSelect.val() : '');
                return;
            }

            $hiddenInput.val(selectedId);

            var hasChildren = $select.find('option:selected').data('has-children');
            if (hasChildren === false || hasChildren === 'false') {
                return;
            }

            abp.ajax({
                url: abp.appPath + 'api/app/organization-unit-lookup/children/' + selectedId,
                type: 'GET'
            }).then(function (result) {
                if (!result.items || result.items.length === 0) {
                    return;
                }

                var level = $container.find('.ou-level-container').length + 1;
                var placeholder = $container.data('select-placeholder') || '';
                var levelLabelTemplate = $container.data('level-label') || 'Level {0}';
                var levelLabel = levelLabelTemplate.replace('{0}', level);

                var $newLevel = $('<div class="mb-2 ou-level-container"></div>').attr('data-level', level);
                $newLevel.append($('<label class="form-label"></label>').html(levelLabel + ' <span>*</span>'));

                var $newSelect = $('<select class="form-select ou-level-select"></select>');
                $newSelect.append($('<option></option>').val('').text(placeholder));

                result.items.forEach(function (item) {
                    $newSelect.append(
                        $('<option></option>')
                            .val(item.id)
                            .text(item.displayName)
                            .attr('data-has-children', item.hasChildren)
                    );
                });

                $newLevel.append($newSelect);
                $container.append($newLevel);
            });
        });
    }
})(jQuery);
