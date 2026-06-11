(function ($) {
    var l = abp.localization.getResource('InternalRequestsManagement');
    var requestService = internalRequestsManagement.requests.request;

    abp.modals.editRequest = function () {
        return {
            initModal: function (publicApi, args) {
                var $modal = publicApi.getModal();
                var $cascade = $modal.find('#edit-ou-cascade');

                internalRequestsManagement.organizationUnitCascade.init(
                    $cascade,
                    editOuInitialPath
                );

                var currentOuId = $modal.find('#EditOrganizationUnitId').val();
                if (currentOuId) {
                    loadRequestTypes($modal, currentOuId, editRequestTypeId);
                }

                $cascade.on('change.ouCascade', '.ou-level-select', function () {
                    var ouId = $modal.find('#EditOrganizationUnitId').val();
                    clearFieldError($cascade);
                    if (ouId) {
                        loadRequestTypes($modal, ouId, null);
                    } else {
                        resetRequestTypes($modal);
                    }
                });

                $modal.find('#EditPrioritySelect').on('change', function () {
                    clearFieldError($(this));
                    updateDynamicFields($modal);
                });

                $modal.find('#EditTypeId').on('change', function () {
                    clearFieldError($(this));
                    updateDynamicFields($modal);
                });

                $modal.find('#EditTitleInput, #EditDescriptionInput, #EditDueDateInput, #EditJustificationInput')
                    .on('input change', function () { clearFieldError($(this)); });

                $modal.find('#EditRequestForm').on('submit', function (e) {
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
        if (!$modal.find('#EditOrganizationUnitId').val()) {
            showFieldError($modal.find('#edit-ou-cascade'), 'Organization unit is required.');
            valid = false;
        }

        // Request Type required
        var $type = $modal.find('#EditTypeId');
        if (!$type.val()) {
            showFieldError($type, 'Request type is required.');
            valid = false;
        }

        // Title: required, min 3, max 256
        var $title = $modal.find('#EditTitleInput');
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
        var $desc = $modal.find('#EditDescriptionInput');
        var desc = $desc.val().trim();
        if (!desc) {
            showFieldError($desc, 'Description is required.');
            valid = false;
        } else if (desc.length > 4000) {
            showFieldError($desc, 'Description must not exceed 4000 characters.');
            valid = false;
        }

        // Priority required
        var $priority = $modal.find('#EditPrioritySelect');
        if (!$priority.val()) {
            showFieldError($priority, 'Priority is required.');
            valid = false;
        }

        var priority = parseInt($priority.val()) || 0;
        var isCritical = priority === 4;

        // Type requirements from selected option
        var $selectedType = $modal.find('#EditTypeId option:selected');
        var typeRequiresJustification = $selectedType.attr('data-requires-justification') === 'true';
        var typeRequiresDueDate = $selectedType.attr('data-requires-due-date') === 'true';

        // Due Date: required if type requires it or Critical; must be future if provided
        var $dueDate = $modal.find('#EditDueDateInput');
        var dueDateVal = $dueDate.val();
        var needsDueDate = typeRequiresDueDate || isCritical;
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
        var $just = $modal.find('#EditJustificationInput');
        var just = $just.val().trim();
        var needsJustification = typeRequiresJustification || isCritical;
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
        $modal.find('#EditResultError').hide();
    }

    function loadRequestTypes($modal, ouId, preselectTypeId) {
        var $select = $modal.find('#EditTypeId');
        $select.prop('disabled', true);

        requestService.getAvailableTypes(ouId).then(function (result) {
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

            if (preselectTypeId) {
                $select.val(preselectTypeId);
            }

            $select.prop('disabled', false);
            updateDynamicFields($modal);
        });
    }

    function resetRequestTypes($modal) {
        var $select = $modal.find('#EditTypeId');
        $select.empty().append('<option value="">' + l('SelectRequestType') + '</option>');
        $select.prop('disabled', true);
        updateDynamicFields($modal);
    }

    function updateDynamicFields($modal) {
        var priority = parseInt($modal.find('#EditPrioritySelect').val());
        var selected = $modal.find('#EditTypeId option:selected');
        var typeRequiresJustification = selected.attr('data-requires-justification') === 'true';
        var typeRequiresDueDate = selected.attr('data-requires-due-date') === 'true';
        var isCritical = priority === 4;

        var needsJustification = typeRequiresJustification || isCritical;
        var needsDueDate = typeRequiresDueDate || isCritical;

        $modal.find('#EditJustificationRequired').toggle(needsJustification);
        $modal.find('#EditJustificationInput').prop('required', needsJustification);
        $modal.find('#EditDueDateRequired').toggle(needsDueDate);
        $modal.find('#EditDueDateInput').prop('required', needsDueDate);
    }

    function saveRequest($modal, publicApi) {
        var id = $modal.find('#EditId').val();
        var input = {
            organizationUnitId: $modal.find('#EditOrganizationUnitId').val(),
            requestTypeId: $modal.find('#EditTypeId').val(),
            title: $modal.find('#EditTitleInput').val(),
            description: $modal.find('#EditDescriptionInput').val(),
            priority: parseInt($modal.find('#EditPrioritySelect').val()),
            dueDate: $modal.find('#EditDueDateInput').val() || null,
            justification: $modal.find('#EditJustificationInput').val() || null
        };

        requestService.update(id, input).then(function (result) {
            if (!result.isSuccess) {
                $modal.find('#EditResultError').text(result.errors[0]?.message).show();
                return;
            }
            abp.notify.success(l('SuccessfullySaved'));
            publicApi.close();
        });
    }

})(jQuery);
