$(document).ready(function () {
    // Inicializar componentes
    initializeFormHandlers();
    initializeDateTimeDefaults();

    function initializeFormHandlers() {
        // Manejar cambio de cliente para cargar mascotas
        $('#customerId').on('change', function () {
            loadPetsByCustomer($(this).val());
        });

        // Manejar clicks de cancelación
        $(document).on('click', '.btn-cancel-order', function () {
            var orderId = $(this).data('order-id');
            var petName = $(this).data('pet-name');
            showCancelConfirmation(orderId, petName);
        });

        // Confirmar cancelación
        $('#confirmCancelBtn').on('click', function () {
            var orderId = $(this).data('order-id');
            cancelOrder(orderId);
        });

        // Validar formulario antes de enviar
        $('#addOrderForm').on('submit', function (e) {
            if (!validateOrderForm()) {
                e.preventDefault();
                return false;
            }
        });
    }

    function loadPetsByCustomer(customerId) {
        var petSelect = $('#petId');

        if (!customerId) {
            petSelect.empty()
                .append('<option value="">Primero selecciona un cliente...</option>')
                .prop('disabled', true);
            return;
        }

        petSelect.empty()
            .append('<option value="">Cargando mascotas...</option>')
            .prop('disabled', true);

        $.ajax({
            url: '/Receptionist/GetPetsByCustomer',
            type: 'GET',
            data: { customerId: customerId },
            success: function (data) {
                petSelect.empty().append('<option value="">Seleccionar mascota...</option>');

                if (data && data.length > 0) {
                    $.each(data, function (index, pet) {
                        var optionText = pet.Name + ' (' + pet.Species + ' - ' + pet.Breed + ')';
                        petSelect.append('<option value="' + pet.Id + '">' + optionText + '</option>');
                    });
                    petSelect.prop('disabled', false);
                } else {
                    petSelect.append('<option value="">No hay mascotas registradas</option>');
                }
            },
            error: function () {
                petSelect.empty().append('<option value="">Error cargando mascotas</option>');
                showAlert('Error al cargar las mascotas. Por favor intente nuevamente.', 'danger');
            }
        });
    }

    function showCancelConfirmation(orderId, petName) {
        $('#cancelPetName').text(petName);
        $('#confirmCancelBtn').data('order-id', orderId);
        $('#cancelOrderModal').modal('show');
    }

    function cancelOrder(orderId) {
        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Receptionist/CancelOrder',
            type: 'POST',
            data: {
                orderId: orderId,
                __RequestVerificationToken: token
            },
            success: function (response) {
                $('#cancelOrderModal').modal('hide');

                if (response.success) {
                    showAlert('Orden cancelada exitosamente.', 'success');
                    setTimeout(function () {
                        location.reload();
                    }, 1500);
                } else {
                    showAlert('Error: ' + response.message, 'danger');
                }
            },
            error: function () {
                $('#cancelOrderModal').modal('hide');
                showAlert('Error al cancelar la orden. Por favor intente nuevamente.', 'danger');
            }
        });
    }

    function validateOrderForm() {
        var isValid = true;
        var errorMessages = [];

        // Validar cliente
        if (!$('#customerId').val()) {
            errorMessages.push('Debe seleccionar un cliente');
            isValid = false;
        }

        // Validar mascota
        if (!$('#petId').val()) {
            errorMessages.push('Debe seleccionar una mascota');
            isValid = false;
        }

        // Validar groomer
        if (!$('#assignedToId').val()) {
            errorMessages.push('Debe asignar la orden a un groomer');
            isValid = false;
        }

        // Validar fecha estimada
        var estimatedReady = $('#estimatedReady').val();
        if (!estimatedReady) {
            errorMessages.push('Debe especificar la hora estimada de finalización');
            isValid = false;
        } else {
            var selectedDate = new Date(estimatedReady);
            var now = new Date();
            if (selectedDate <= now) {
                errorMessages.push('La hora estimada debe ser futura');
                isValid = false;
            }
        }

        // Validar servicios
        var selectedServices = $('input[name="ServiceIds"]:checked').length;
        if (selectedServices === 0) {
            errorMessages.push('Debe seleccionar al menos un servicio');
            isValid = false;
        }

        if (!isValid) {
            showAlert('Por favor corrija los siguientes errores:\n• ' + errorMessages.join('\n• '), 'danger');
        }

        return isValid;
    }

    function initializeDateTimeDefaults() {
        // Configurar hora mínima para estimatedReady (1 hora desde ahora)
        var now = new Date();
        now.setHours(now.getHours() + 1);
        now.setMinutes(0, 0, 0); // Redondear a la hora completa

        var formattedDateTime = now.toISOString().slice(0, 16);
        $('#estimatedReady').attr('min', formattedDateTime);

        // Configurar valor por defecto (2 horas desde ahora)
        now.setHours(now.getHours() + 1);
        var defaultDateTime = now.toISOString().slice(0, 16);
        $('#estimatedReady').val(defaultDateTime);
    }

    function showAlert(message, type) {
        var alertClass = 'alert-' + type;
        var alertHtml = '<div class="alert ' + alertClass + ' alert-dismissible fade show" role="alert">' +
            message.replace(/\n/g, '<br>') +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';

        // Remover alertas existentes
        $('.alert').remove();

        // Agregar nueva alerta al principio del contenido
        $('.main-content').prepend(alertHtml);

        // Auto-ocultar después de 5 segundos para alertas de éxito
        if (type === 'success') {
            setTimeout(function () {
                $('.alert-success').fadeOut();
            }, 5000);
        }
    }

    // Limpiar formulario cuando se cierra el modal
    $('#addOrderModal').on('hidden.bs.modal', function () {
        $('#addOrderForm')[0].reset();
        $('#petId').empty().append('<option value="">Primero selecciona un cliente...</option>').prop('disabled', true);
        initializeDateTimeDefaults();
    });
});