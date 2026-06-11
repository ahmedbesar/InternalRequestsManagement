(function ($) {
    var l = abp.localization.getResource('InternalRequestsManagement');
    var requestService = internalRequestsManagement.requests.request;
    var selectedTypeRequiresJustification = false;
    var selectedTypeRequiresDueDate = false;

    abp.modals.createRequest = function () {
        return {
            initModal: function (publicApi, args) {
                var $modal = publicApi.getModal();

                internalRequestsManagement.organizationUnitCascade.init($modal.find('#ou-cascade'));

                $modal.find('#ou-cascade').on('change.ouCascade', '.ou-level-select', function () {
                    var ouId = $modal.find('#OrganizationUnitId').val();
                    clearFieldError($modal.find('#ou-cascade'));
                    if (ouId) {
                        loadRequestTypes($modal, ouId);
                    } else {
                        resetRequestTypes($modal);
                    }
                });

                $modal.find('#PrioritySelect').on('change', function () {
                    clearFieldError($(this));
                    updateDynamicFields($modal);
                });

                $modal.find('#TypeId').on('change', function () {
                    clearFieldError($(this));
                    var selected = $(this).find('option:selected');
                    selectedTypeRequiresJustification = selected.data('requires-justification') === true;
                    selectedTypeRequiresDueDate = selected.data('requires-due-date') === true;
                    updateDynamicFields($modal);
                });

                $modal.find('#TitleInput, #DescriptionInput, #DueDateInput, #JustificationInput')
                    .on('input change', function () { clearFieldError($(this)); });

                $modal.find('#CreateRequestForm').on('submit', function (e) {
                    e.preventDefault();
                    if (validate($modal)) {
                        saveRequest($modal, publicApi);
                    }
                });
            }
        };
    };

    // ── Validation ────────────────────────────────────────────────────────────

    function validate($modal) {
        clearAllErrors($modal);
        var valid = true;

        // Organization Unit required
        if (!$modal.find('#OrganizationUnitId').val()) {
            showFieldError($modal.find('#ou-cascade'), 'Organization unit is required.');
            valid = false;
        }

        // Request Type required
        var $type = $modal.find('#TypeId');
        if (!$type.val()) {
            showFieldError($type, 'Request type is required.');
            valid = false;
        }

        // Title: required, min 3, max 256
        var $title = $modal.find('#TitleInput');
        var title = $title.val().trim();
        if (!title) {
            showFieldError($title, 'Title is required.');
            valid = false;
        } else if (title.length < 3) {
            showFieldError($title, 'Title must be at least 3 characters.');
            valid = false;
        } else if (title.length > 256) {
            showFieldError($title, 'Title must not exceed 256 characters.');
            valid = false;
        }

        // Description: required, max 4000
        var $desc = $modal.find('#DescriptionInput');
        var desc = $desc.val().trim();
        if (!desc) {
            showFieldError($desc, 'Description is required.');
            valid = false;
        } else if (desc.length > 4000) {
            showFieldError($desc, 'Description must not exceed 4000 characters.');
            valid = false;
        }

        // Priority required
        var $priority = $modal.find('#PrioritySelect');
        if (!$priority.val()) {
            showFieldError($priority, 'Priority is required.');
            valid = false;
        }

        var priority = parseInt($priority.val()) || 0;
        var isCritical = priority === 4;

        // Due Date: required if type requires it or Critical; must be future if provided
        var $dueDate = $modal.find('#DueDateInput');
        var dueDateVal = $dueDate.val();
        var needsDueDate = selectedTypeRequiresDueDate || isCritical;
        if (needsDueDate && !dueDateVal) {
            showFieldError($dueDate, 'Due date is required for this request type or priority.');
            valid = false;
        } else if (dueDateVal) {
            var today = new Date(); today.setHours(0, 0, 0, 0);
            var selected = new Date(dueDateVal);
            if (selected <= today) {
                showFieldError($dueDate, 'Due date must be in the future.');
                valid = false;
            }
        }

        // Justification: required if type requires it or Critical; max 2000
        var $just = $modal.find('#JustificationInput');
        var just = $just.val().trim();
        var needsJustification = selectedTypeRequiresJustification || isCritical;
        if (needsJustification && !just) {
            showFieldError($just, 'Justification is required for this request type or priority.');
            valid = false;
        } else if (just.length > 2000) {
            showFieldError($just, 'Justification must not exceed 2000 characters.');
            valid = false;
        }

        return valid;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    function showFieldError($field, message) {
        $field.addClass('is-invalid');
        var $err = $field.next('.js-field-error');
        if (!$err.length) {
            $err = $('<div class="invalid-feedback js-field-error" style="display:block"></div>')
                .insertAfter($field);
        }
        $err.text(message).show();
    }

    function clearFieldError($field) {
        $field.removeClass('is-invalid');
        $field.next('.js-field-error').hide();
    }

    function clearAllErrors($modal) {
        $modal.find('.is-invalid').removeClass('is-invalid');
        $modal.find('.js-field-error').hide();
        $modal.find('#CreateResultError').hide();
    }

    function loadRequestTypes($modal, ouId) {
        requestService.getAvailableTypes(ouId).then(function (result) {
            var $select = $modal.find('#TypeId');
            $select.empty().append('<option value="">' + l('SelectRequestType') + '</option>');
            $.each(result.items, function (_, t) {
                $select.append(
                    $('<option></option>')
                        .val(t.id)
                        .text(t.name)
                        .attr('data-requires-justification', t.requiresJustification)
                        .attr('data-requires-due-date', t.requiresDueDate)
                );
            });
            $select.prop('disabled', false);
        });
    }

    function resetRequestTypes($modal) {
        var $select = $modal.find('#TypeId');
        $select.empty().append('<option value="">' + l('SelectRequestType') + '</option>');
        $select.prop('disabled', true);
        selectedTypeRequiresJustification = false;
        selectedTypeRequiresDueDate = false;
        updateDynamicFields($modal);
    }

    function updateDynamicFields($modal) {
        var priority = parseInt($modal.find('#PrioritySelect').val());
        var isCritical = priority === 4;

        var needsJustification = selectedTypeRequiresJustification || isCritical;
        var needsDueDate = selectedTypeRequiresDueDate || isCritical;

        $modal.find('#JustificationRequired').toggle(needsJustification);
        $modal.find('#JustificationHint').toggle(needsJustification);
        $modal.find('#JustificationInput').prop('required', needsJustification);

        $modal.find('#DueDateRequired').toggle(needsDueDate);
        $modal.find('#DueDateInput').prop('required', needsDueDate);
    }

    function saveRequest($modal, publicApi) {
        var input = {
            organizationUnitId: $modal.find('#OrganizationUnitId').val(),
            requestTypeId: $modal.find('#TypeId').val(),
            title: $modal.find('#TitleInput').val(),
            description: $modal.find('#DescriptionInput').val(),
            priority: parseInt($modal.find('#PrioritySelect').val()),
            dueDate: $modal.find('#DueDateInput').val() || null,
            justification: $modal.find('#JustificationInput').val() || null
        };

        requestService.create(input).then(function (result) {
            if (!result.isSuccess) {
                $modal.find('#CreateResultError').text(result.errors[0]?.message).show();
                return;
            }
            abp.notify.success(l('SuccessfullySaved'));
            publicApi.close();
        });
    }

})(jQuery);
