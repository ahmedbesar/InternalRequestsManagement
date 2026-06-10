(function ($) {
    window.internalRequestsManagement = window.internalRequestsManagement || {};

    internalRequestsManagement.organizationUnitCascade = {
        init: function ($container, initialPath) {
            if (!$container.length) {
                return;
            }

            $container.off('change.ouCascade').on('change.ouCascade', '.ou-level-select', function () {
                onLevelChange($container, $(this));
            });

            if (initialPath && initialPath.length > 0) {
                initializeWithPath($container, initialPath);
            }
        }
    };

    function onLevelChange($container, $select) {
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

        loadChildrenLevel($container, selectedId);
    }

    function loadChildrenLevel($container, parentId) {
        return abp.ajax({
            url: abp.appPath + 'api/app/organization-unit-lookup/children/' + parentId,
            type: 'GET'
        }).then(function (result) {
            if (!result.items || result.items.length === 0) {
                return;
            }

            appendLevel($container, result.items);
        });
    }

    function appendLevel($container, items) {
        var level = $container.find('.ou-level-container').length + 1;
        var placeholder = $container.data('select-placeholder') || '';
        var levelLabelTemplate = $container.data('level-label') || 'Level {0}';
        var levelLabel = levelLabelTemplate.replace('{0}', level);

        var $newLevel = $('<div class="mb-2 ou-level-container"></div>').attr('data-level', level);
        $newLevel.append($('<label class="form-label"></label>').html(levelLabel + ' <span>*</span>'));

        var $newSelect = $('<select class="form-select ou-level-select"></select>');
        $newSelect.append($('<option></option>').val('').text(placeholder));

        items.forEach(function (item) {
            $newSelect.append(
                $('<option></option>')
                    .val(item.id)
                    .text(item.displayName)
                    .attr('data-has-children', item.hasChildren)
            );
        });

        $newLevel.append($newSelect);
        $container.append($newLevel);
    }

    function initializeWithPath($container, pathIds) {
        var chain = Promise.resolve();

        pathIds.forEach(function (id, index) {
            chain = chain.then(function () {
                var $levels = $container.find('.ou-level-container');
                var $select = $levels.eq(index).find('.ou-level-select');
                if (!$select.length) {
                    return Promise.resolve();
                }

                $select.val(id);
                $container.find('#OrganizationUnitId').val(id);

                if (index >= pathIds.length - 1) {
                    return Promise.resolve();
                }

                var hasChildren = $select.find('option:selected').data('has-children');
                if (hasChildren === false || hasChildren === 'false') {
                    return Promise.resolve();
                }

                return loadChildrenLevel($container, id).then(function () {
                    return Promise.resolve();
                });
            });
        });

        return chain;
    }
})(jQuery);
