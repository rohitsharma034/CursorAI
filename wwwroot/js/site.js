// Professional Inmate Search Application JavaScript

$(document).ready(function() {
    // Navbar scroll effect
    $(window).scroll(function() {
        var scroll = $(window).scrollTop();
        if (scroll >= 50) {
            $('.navbar').addClass('scrolled');
        } else {
            $('.navbar').removeClass('scrolled');
        }
    });

    // Smooth scrolling for navigation links
    $('a[href^="#"]').on('click', function(event) {
        var target = $(this.getAttribute('href'));
        if (target.length) {
            event.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 80
            }, 1000);
        }
    });

    // Form submission handling
    $('#inmateSearchForm').on('submit', function() {
        $('#searchButton').prop('disabled', true);
        $('#searchButton').html('<i class="fas fa-spinner fa-spin me-2"></i>Processing...');
        
        // Show loading modal
        $('#loadingModal').modal('show');
    });

    // Auto-hide alerts after 8 seconds
    setTimeout(function() {
        $('.alert').fadeOut('slow');
    }, 8000);

    // Add animation to cards on scroll
    $(window).scroll(function() {
        $('.card').each(function() {
            var elementTop = $(this).offset().top;
            var elementBottom = elementTop + $(this).outerHeight();
            var viewportTop = $(window).scrollTop();
            var viewportBottom = viewportTop + $(window).height();

            if (elementBottom > viewportTop && elementTop < viewportBottom) {
                $(this).addClass('animate__animated animate__fadeInUp');
            }
        });
    });

    // Navbar mobile toggle enhancement
    $('.navbar-toggler').on('click', function() {
        $(this).toggleClass('active');
    });

    // Close mobile menu when clicking on a link
    $('.navbar-nav .nav-link').on('click', function() {
        if ($(window).width() < 992) {
            $('.navbar-collapse').collapse('hide');
        }
    });

    // Add loading state to buttons
    $('.btn').on('click', function() {
        if (!$(this).hasClass('btn-loading')) {
            $(this).addClass('btn-loading');
            setTimeout(() => {
                $(this).removeClass('btn-loading');
            }, 2000);
        }
    });

    // Form validation enhancement
    $('.form-control').on('blur', function() {
        if ($(this).val().trim() === '') {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid').addClass('is-valid');
        }
    });

    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();

    // Initialize popovers
    $('[data-bs-toggle="popover"]').popover();
});
