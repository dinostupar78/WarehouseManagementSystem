// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    // Client-side validation setup.
    $("form").each(function () {
        if ($.validator && $.validator.unobtrusive) {
            $.validator.unobtrusive.parse(this);
        }
    });

    // Validate fields when the user leaves or changes them.
    $("form").on("blur change", "input, select, textarea", function () {
        const $field = $(this);
        const $form = $field.closest("form");

        if (!$form.data("validator")) {
            return;
        }

        $field.valid();
    });

    // Re-check invalid fields while the user corrects them.
    $("form").on("keyup", "input, textarea", function () {
        const $field = $(this);

        if ($field.hasClass("input-validation-error")) {
            $field.valid();
        }
    });

    // Global page loading animation.
    const pageLoaderDelay = 180;

    function showPageLoader() {
        const $loader = $("[data-page-loader]");

        if (!$loader.length || $loader.hasClass("is-visible")) {
            return;
        }

        $loader.prop("hidden", false);

        requestAnimationFrame(function () {
            $loader.addClass("is-visible");
        });
    }

    function hidePageLoader() {
        const $loader = $("[data-page-loader]");

        $loader.removeClass("is-visible").prop("hidden", true);
    }

    function shouldShowPageLoader(link, event) {
        const href = link.getAttribute("href");
        const target = link.getAttribute("target");

        if (event.isDefaultPrevented() || event.which > 1 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) {
            return false;
        }

        if (!href || href.startsWith("#") || href.startsWith("javascript:") || href.startsWith("mailto:") || href.startsWith("tel:")) {
            return false;
        }

        if (target && target.toLowerCase() !== "_self") {
            return false;
        }

        if (link.hasAttribute("download") || link.hasAttribute("data-no-page-loader")) {
            return false;
        }

        const url = new URL(href, window.location.href);

        if (url.origin !== window.location.origin) {
            return false;
        }

        return url.pathname !== window.location.pathname ||
            url.search !== window.location.search ||
            url.hash === "";
    }

    $(window).on("pageshow", hidePageLoader);

    $(document).on("click", "a[href]", function (event) {
        if (shouldShowPageLoader(this, event)) {
            event.preventDefault();
            showPageLoader();

            const nextUrl = this.href;
            setTimeout(function () {
                window.location.href = nextUrl;
            }, pageLoaderDelay);
        }
    });

    // AJAX table search with skeleton and row animations.
    function getTableColumnCount($target) {
        return $target.closest("table").find("thead th").length || 1;
    }

    function buildSearchSkeletonRows() {
        const $rows = $("<div>").addClass("wms-skeleton-rows");

        for (let i = 0; i < 3; i++) {
            $("<div>")
                .addClass("wms-skeleton-row")
                .append($("<span>").addClass("wms-skeleton-cell wms-skeleton-cell-wide"))
                .append($("<span>").addClass("wms-skeleton-cell wms-skeleton-cell-medium"))
                .append($("<span>").addClass("wms-skeleton-cell wms-skeleton-cell-small"))
                .appendTo($rows);
        }

        return $rows;
    }

    function showSearchSkeleton($target) {
        const $container = $target.closest(".wms-table-container");

        if (!$container.length) {
            return;
        }

        $container.addClass("wms-search-loading");

        if (!$container.find(".wms-skeleton-overlay").length) {
            $("<div>")
                .addClass("wms-skeleton-overlay")
                .append(
                    $("<div>")
                        .addClass("wms-skeleton-status")
                        .append($("<span>").addClass("wms-loading-spinner"))
                        .append($("<span>").text("Loading results..."))
                )
                .append(buildSearchSkeletonRows())
                .appendTo($container);
        }
    }

    function hideSearchSkeleton($target) {
        const $container = $target.closest(".wms-table-container");

        $container
            .removeClass("wms-search-loading")
            .find(".wms-skeleton-overlay")
            .remove();
    }

    function animateSearchRows($target) {
        const $rows = $target.find("tr");

        $rows.each(function () {
            const $row = $(this);

            if ($row.find("td[colspan]").length) {
                $row.addClass("wms-empty-row");
            }
        });

        $rows.addClass("wms-table-row-enter");

        setTimeout(function () {
            $rows.removeClass("wms-table-row-enter");
        }, 420);
    }

    function showSearchResults($target, html) {
        hideSearchSkeleton($target);

        $target
            .html(html)
            .removeClass("wms-list-fade-out");

        animateSearchRows($target);
    }

    // Send search request and replace table body with returned partial view.
    function runAjaxSearch($input) {
        const $target = $($input.data("target"));
        const url = $input.data("search-url");
        const term = $input.val();

        if (!$target.length || !url) {
            return;
        }

        const activeRequest = $input.data("active-request");
        if (activeRequest) {
            activeRequest.abort();
        }

        clearTimeout($input.data("skeleton-timer"));

        $target.addClass("wms-list-fade-out");

        const request = $.get(url, { term: term })
            .done(function (html) {
                clearTimeout($input.data("skeleton-timer"));
                showSearchResults($target, html);
            })
            .fail(function (_xhr, status) {
                clearTimeout($input.data("skeleton-timer"));
                hideSearchSkeleton($target);

                if (status !== "abort") {
                    const columnCount = getTableColumnCount($target);
                    $target
                        .removeClass("wms-list-fade-out")
                        .html("<tr><td colspan=\"" + columnCount + "\" class=\"text-center py-4 text-danger border-0 fw-bold\">Search failed. Try again.</td></tr>");

                    animateSearchRows($target);
                }
            });

        $input.data("active-request", request);
        $input.data("skeleton-timer", setTimeout(function () {
            if (request.readyState === 4) {
                return;
            }

            showSearchSkeleton($target);
        }, 260));
    }

    // Debounce search input so AJAX is not called on every keystroke.
    $("[data-ajax-search]").on("input", function () {
        const $input = $(this);
        const timer = $input.data("search-timer");

        clearTimeout(timer);
        $input.data("search-timer", setTimeout(function () {
            runAjaxSearch($input);
        }, 250));
    });

    // Custom AJAX autocomplete dropdown.
    function closeAutocomplete($widget) {
        $widget.find("[data-autocomplete-results]").prop("hidden", true).empty();
        $widget.find("[data-autocomplete-input]").attr("aria-expanded", "false");
    }

    function setAutocompleteError($widget, message) {
        $widget.find("[data-autocomplete-input]")
            .addClass("input-validation-error")
            .attr("aria-invalid", "true");

        $widget.find("[data-autocomplete-error]")
            .removeClass("field-validation-valid")
            .addClass("field-validation-error")
            .text(message);

        $widget.addClass("wms-autocomplete-invalid");
        setTimeout(function () {
            $widget.removeClass("wms-autocomplete-invalid");
        }, 350);
    }

    function clearAutocompleteError($widget) {
        $widget.find("[data-autocomplete-input]")
            .removeClass("input-validation-error")
            .attr("aria-invalid", "false");

        $widget.find("[data-autocomplete-error]")
            .removeClass("field-validation-error")
            .addClass("field-validation-valid")
            .text("");
    }

    function validateAutocomplete($widget) {
        const isRequired = $widget.data("required") === true || $widget.data("required") === "true";
        const selectedValue = $.trim($widget.find("[data-autocomplete-value]").val());
        const displayValue = $.trim($widget.find("[data-autocomplete-input]").val());

        if (!selectedValue && isRequired && !displayValue) {
            setAutocompleteError($widget, $widget.data("empty-message"));
            return false;
        }

        if (!selectedValue && displayValue) {
            setAutocompleteError($widget, $widget.data("invalid-message"));
            return false;
        }

        clearAutocompleteError($widget);
        return true;
    }

    // Render JSON results returned from autocomplete endpoint.
    function renderAutocompleteResults($widget, items) {
        const $results = $widget.find("[data-autocomplete-results]");
        $results.empty();

        if (!items || !items.length) {
            $("<div>")
                .addClass("wms-autocomplete-empty")
                .text("No matches found.")
                .appendTo($results);
        } else {
            items.forEach(function (item, index) {
                const optionText = item.text || item.id;
                const optionSubtitle = item.subtitle || item.description || "";

                const $option = $("<button>")
                    .attr("type", "button")
                    .attr("role", "option")
                    .addClass("wms-autocomplete-option")
                    .css("--wms-option-index", index)
                    .data("id", item.id)
                    .data("text", optionText);

                $("<span>")
                    .addClass("wms-autocomplete-option-title")
                    .text(optionText)
                    .appendTo($option);

                if (optionSubtitle) {
                    $("<span>")
                        .addClass("wms-autocomplete-option-subtitle")
                        .text(optionSubtitle)
                        .appendTo($option);
                }

                $option.appendTo($results);
            });
        }

        $results.prop("hidden", false);
        $widget.find("[data-autocomplete-input]").attr("aria-expanded", "true");
    }

    // Load autocomplete options from server.
    function fetchAutocompleteResults($widget) {
        const url = $widget.data("search-url");
        const term = $widget.find("[data-autocomplete-input]").val();

        if (!url) {
            return;
        }

        const activeRequest = $widget.data("active-request");
        if (activeRequest) {
            activeRequest.abort();
        }

        const request = $.getJSON(url, { term: term })
            .done(function (items) {
                renderAutocompleteResults($widget, items);
            })
            .fail(function (_xhr, status) {
                if (status !== "abort") {
                    renderAutocompleteResults($widget, []);
                }
            });

        $widget.data("active-request", request);
    }

    $("[data-autocomplete]").each(function () {
        const $widget = $(this);
        $widget.data("search-timer", null);
    });

    $(document).on("input", "[data-autocomplete-input]", function () {
        const $widget = $(this).closest("[data-autocomplete]");
        const timer = $widget.data("search-timer");

        clearTimeout(timer);
        $widget.find("[data-autocomplete-value]").val("");

        clearAutocompleteError($widget);
        $widget.data("search-timer", setTimeout(function () {
            fetchAutocompleteResults($widget);
        }, 250));
    });

    $(document).on("focus", "[data-autocomplete-input]", function () {
        const $widget = $(this).closest("[data-autocomplete]");

        if (!$(this).val()) {
            fetchAutocompleteResults($widget);
        }
    });

    $(document).on("click", "[data-autocomplete-toggle]", function () {
        const $widget = $(this).closest("[data-autocomplete]");
        fetchAutocompleteResults($widget);
    });

    // Store selected autocomplete text for display and id for form submit.
    $(document).on("click", ".wms-autocomplete-option", function () {
        const $option = $(this);
        const $widget = $option.closest("[data-autocomplete]");

        $widget.data("is-selecting", true);
        $option.addClass("is-selected");
        $widget.find("[data-autocomplete-value]").val($option.data("id")).trigger("change");
        $widget.find("[data-autocomplete-input]").val($option.data("text"));

        validateAutocomplete($widget);
        $widget.addClass("wms-autocomplete-selected");
        setTimeout(function () {
            $widget.removeClass("wms-autocomplete-selected");
        }, 800);

        setTimeout(function () {
            closeAutocomplete($widget);
            $widget.data("is-selecting", false);
        }, 180);
    });

    $(document).on("blur", "[data-autocomplete-input]", function () {
        const $widget = $(this).closest("[data-autocomplete]");

        setTimeout(function () {
            if ($widget.data("is-selecting")) {
                return;
            }

            if (!$widget.find(":focus").length) {
                validateAutocomplete($widget);
                closeAutocomplete($widget);
            }
        }, 150);
    });

    // Validate custom controls before submitting the form.
    $("form").on("submit", function (event) {
        const $form = $(this);
        let isValid = true;

        if ($form.data("page-loader-submitted")) {
            return;
        }

        $form.find("[data-autocomplete]").each(function () {
            if (!validateAutocomplete($(this))) {
                isValid = false;
            }
        });

        $form.find("[data-datetime-picker]").each(function () {
            if (!validateDateTimePicker($(this))) {
                isValid = false;
            }
        });

        if ($form.data("validator") && !$form.valid()) {
            isValid = false;
        }

        if (!isValid) {
            event.preventDefault();
            return;
        }

        if (!$form.is("[data-no-page-loader]")) {
            event.preventDefault();
            showPageLoader();

            const submitter = event.originalEvent && event.originalEvent.submitter;

            if (submitter) {
                if (submitter.formAction) {
                    $form.attr("action", submitter.formAction);
                }

                if (submitter.formMethod) {
                    $form.attr("method", submitter.formMethod);
                }

                if (submitter.name) {
                    $("<input>")
                        .attr("type", "hidden")
                        .attr("name", submitter.name)
                        .val(submitter.value)
                        .appendTo($form);
                }
            }

            $form.data("page-loader-submitted", true);
            setTimeout(function () {
                $form[0].submit();
            }, pageLoaderDelay);
        }
    });

    // Custom date-time picker with hr/en formatting.
    function isCroatianCulture() {
        const languages = navigator.languages && navigator.languages.length
            ? navigator.languages
            : [navigator.language || "en"];

        return languages[0].toLowerCase().startsWith("hr");
    }

    function pad2(value) {
        return value.toString().padStart(2, "0");
    }

    function parseIsoDateTime(value) {
        const match = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/.exec(value || "");

        if (!match) {
            return null;
        }

        const year = Number(match[1]);
        const month = Number(match[2]) - 1;
        const day = Number(match[3]);
        const hour = Number(match[4]);
        const minute = Number(match[5]);
        const date = new Date(year, month, day, hour, minute);

        return isRealDate(date, year, month, day, hour, minute) ? date : null;
    }

    function toIsoDateTime(date) {
        return date.getFullYear() + "-" +
            pad2(date.getMonth() + 1) + "-" +
            pad2(date.getDate()) + "T" +
            pad2(date.getHours()) + ":" +
            pad2(date.getMinutes());
    }

    function formatDateTime(date) {
        if (isCroatianCulture()) {
            return pad2(date.getDate()) + "." +
                pad2(date.getMonth() + 1) + "." +
                date.getFullYear() + ". " +
                pad2(date.getHours()) + ":" +
                pad2(date.getMinutes());
        }

        const hour24 = date.getHours();
        const amPm = hour24 >= 12 ? "PM" : "AM";
        const hour12 = hour24 % 12 || 12;

        return pad2(date.getMonth() + 1) + "/" +
            pad2(date.getDate()) + "/" +
            date.getFullYear() + " " +
            hour12 + ":" +
            pad2(date.getMinutes()) + " " +
            amPm;
    }

    function parseTypedDateTime(value) {
        const text = $.trim(value);
        let match;

        if (!text) {
            return null;
        }

        match = /^(\d{1,2})\.(\d{1,2})\.(\d{4})\.?\s+(\d{1,2}):(\d{2})$/.exec(text);
        if (match) {
            const year = Number(match[3]);
            const month = Number(match[2]) - 1;
            const day = Number(match[1]);
            const hour = Number(match[4]);
            const minute = Number(match[5]);
            const date = new Date(year, month, day, hour, minute);

            return isRealDate(date, year, month, day, hour, minute) ? date : null;
        }

        match = /^(\d{1,2})\/(\d{1,2})\/(\d{4})\s+(\d{1,2}):(\d{2})\s*(AM|PM)?$/i.exec(text);
        if (match) {
            const year = Number(match[3]);
            const month = Number(match[1]) - 1;
            const day = Number(match[2]);
            let hour = Number(match[4]);
            const minute = Number(match[5]);
            const amPm = (match[6] || "").toUpperCase();

            if (amPm === "PM" && hour < 12) {
                hour += 12;
            }

            if (amPm === "AM" && hour === 12) {
                hour = 0;
            }

            const date = new Date(year, month, day, hour, minute);

            return isRealDate(date, year, month, day, hour, minute) ? date : null;
        }

        return parseIsoDateTime(text);
    }

    // Prevent invalid dates such as 31.02.
    function isRealDate(date, year, month, day, hour, minute) {
        return date instanceof Date &&
            !Number.isNaN(date.getTime()) &&
            date.getFullYear() === year &&
            date.getMonth() === month &&
            date.getDate() === day &&
            date.getHours() === hour &&
            date.getMinutes() === minute;
    }

    function getDateTimeState($widget) {
        let state = $widget.data("datetime-state");

        if (!state) {
            const existingDate = parseIsoDateTime($widget.find("[data-datetime-value]").val());
            const today = existingDate || new Date();

            state = {
                selectedDate: existingDate,
                viewYear: today.getFullYear(),
                viewMonth: today.getMonth()
            };

            $widget.data("datetime-state", state);
        }

        return state;
    }

    function setDateTimeError($widget, message) {
        $widget.find("[data-datetime-display]")
            .addClass("input-validation-error")
            .attr("aria-invalid", "true");

        $widget.find("[data-datetime-error]")
            .removeClass("field-validation-valid")
            .addClass("field-validation-error")
            .text(message);
    }

    function clearDateTimeError($widget) {
        $widget.find("[data-datetime-display]")
            .removeClass("input-validation-error")
            .attr("aria-invalid", "false");

        $widget.find("[data-datetime-error]")
            .removeClass("field-validation-error")
            .addClass("field-validation-valid")
            .text("");
    }

    function setDateTimeValue($widget, date) {
        const state = getDateTimeState($widget);

        state.selectedDate = date;
        state.viewYear = date.getFullYear();
        state.viewMonth = date.getMonth();

        $widget.find("[data-datetime-value]").val(toIsoDateTime(date)).trigger("change");
        $widget.find("[data-datetime-display]").val(formatDateTime(date));
        $widget.find("[data-datetime-hour]").val(pad2(date.getHours()));
        $widget.find("[data-datetime-minute]").val(pad2(date.getMinutes()));

        clearDateTimeError($widget);
        renderDateTimePicker($widget);
    }

    function clearDateTimeValue($widget) {
        const state = getDateTimeState($widget);

        state.selectedDate = null;
        $widget.find("[data-datetime-value]").val("").trigger("change");
        $widget.find("[data-datetime-display]").val("");
        renderDateTimePicker($widget);
    }

    function renderDateTimePicker($widget) {
        const state = getDateTimeState($widget);
        const $days = $widget.find("[data-datetime-days]");
        const $weekdays = $widget.find("[data-datetime-weekdays]");
        const monthName = new Date(state.viewYear, state.viewMonth, 1)
            .toLocaleString(isCroatianCulture() ? "hr-HR" : "en-US", { month: "long", year: "numeric" });
        const weekDays = isCroatianCulture()
            ? ["Pon", "Uto", "Sri", "Cet", "Pet", "Sub", "Ned"]
            : ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
        const firstDay = new Date(state.viewYear, state.viewMonth, 1);
        const daysInMonth = new Date(state.viewYear, state.viewMonth + 1, 0).getDate();
        const startOffset = isCroatianCulture()
            ? (firstDay.getDay() + 6) % 7
            : firstDay.getDay();

        $widget.find("[data-datetime-title]").text(monthName);
        $weekdays.empty();
        $days.empty();

        weekDays.forEach(function (day) {
            $("<span>").text(day).appendTo($weekdays);
        });

        for (let i = 0; i < startOffset; i++) {
            $("<span>").addClass("wms-datetime-empty-day").appendTo($days);
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const isSelected = state.selectedDate &&
                state.selectedDate.getFullYear() === state.viewYear &&
                state.selectedDate.getMonth() === state.viewMonth &&
                state.selectedDate.getDate() === day;

            $("<button>")
                .attr("type", "button")
                .addClass("wms-datetime-day")
                .toggleClass("is-selected", !!isSelected)
                .text(day)
                .data("day", day)
                .appendTo($days);
        }

        const displayDate = state.selectedDate || new Date(state.viewYear, state.viewMonth, 1, 8, 0);
        $widget.find("[data-datetime-hour]").val(pad2(displayDate.getHours()));
        $widget.find("[data-datetime-minute]").val(pad2(displayDate.getMinutes()));
    }

    function openDateTimePicker($widget) {
        renderDateTimePicker($widget);
        $widget.find("[data-datetime-menu]").prop("hidden", false);
        $widget.find("[data-datetime-display]").attr("aria-expanded", "true");
    }

    function closeDateTimePicker($widget) {
        $widget.find("[data-datetime-menu]").prop("hidden", true);
        $widget.find("[data-datetime-display]").attr("aria-expanded", "false");
    }

    // Validate typed or selected date-time value.
    function validateDateTimePicker($widget) {
        const isRequired = $widget.data("required") === true || $widget.data("required") === "true";
        const displayValue = $.trim($widget.find("[data-datetime-display]").val());
        const hiddenValue = $.trim($widget.find("[data-datetime-value]").val());

        if (!displayValue && !hiddenValue) {
            if (isRequired) {
                setDateTimeError($widget, $widget.data("empty-message"));
                return false;
            }

            clearDateTimeError($widget);
            return true;
        }

        const date = parseTypedDateTime(displayValue || hiddenValue);
        if (!date) {
            setDateTimeError($widget, $widget.data("invalid-message"));
            return false;
        }

        setDateTimeValue($widget, date);
        return true;
    }

    $("[data-datetime-picker]").each(function () {
        const $widget = $(this);
        const existingDate = parseIsoDateTime($widget.find("[data-datetime-value]").val());
        const placeholder = isCroatianCulture() ? "dd.MM.yyyy. HH:mm" : "MM/dd/yyyy h:mm AM/PM";
        const initialError = $.trim($widget.find("[data-datetime-error]").text());

        if (!$widget.find("[data-datetime-display]").attr("placeholder")) {
            $widget.find("[data-datetime-display]").attr("placeholder", placeholder);
        }

        if (existingDate) {
            setDateTimeValue($widget, existingDate);
        } else {
            renderDateTimePicker($widget);
        }

        if (initialError) {
            setDateTimeError($widget, initialError);
        }
    });

    $(document).on("click", "[data-datetime-toggle]", function () {
        const $widget = $(this).closest("[data-datetime-picker]");
        const isOpen = !$widget.find("[data-datetime-menu]").prop("hidden");

        if (isOpen) {
            closeDateTimePicker($widget);
        } else {
            openDateTimePicker($widget);
        }
    });

    $(document).on("click", "[data-datetime-prev]", function () {
        const $widget = $(this).closest("[data-datetime-picker]");
        const state = getDateTimeState($widget);

        state.viewMonth--;
        if (state.viewMonth < 0) {
            state.viewMonth = 11;
            state.viewYear--;
        }

        renderDateTimePicker($widget);
    });

    $(document).on("click", "[data-datetime-next]", function () {
        const $widget = $(this).closest("[data-datetime-picker]");
        const state = getDateTimeState($widget);

        state.viewMonth++;
        if (state.viewMonth > 11) {
            state.viewMonth = 0;
            state.viewYear++;
        }

        renderDateTimePicker($widget);
    });

    $(document).on("click", ".wms-datetime-day", function () {
        const $widget = $(this).closest("[data-datetime-picker]");
        const state = getDateTimeState($widget);
        const hour = Number($widget.find("[data-datetime-hour]").val()) || 0;
        const minute = Number($widget.find("[data-datetime-minute]").val()) || 0;

        setDateTimeValue($widget, new Date(state.viewYear, state.viewMonth, Number($(this).data("day")), hour, minute));
    });

    $(document).on("change", "[data-datetime-hour], [data-datetime-minute]", function () {
        const $widget = $(this).closest("[data-datetime-picker]");
        const state = getDateTimeState($widget);
        const baseDate = state.selectedDate || new Date(state.viewYear, state.viewMonth, 1);
        const hour = Math.min(23, Math.max(0, Number($widget.find("[data-datetime-hour]").val()) || 0));
        const minute = Math.min(59, Math.max(0, Number($widget.find("[data-datetime-minute]").val()) || 0));

        setDateTimeValue($widget, new Date(baseDate.getFullYear(), baseDate.getMonth(), baseDate.getDate(), hour, minute));
    });

    $(document).on("click", "[data-datetime-today]", function () {
        setDateTimeValue($(this).closest("[data-datetime-picker]"), new Date());
    });

    $(document).on("click", "[data-datetime-clear]", function () {
        clearDateTimeValue($(this).closest("[data-datetime-picker]"));
    });

    $(document).on("click", "[data-datetime-apply]", function () {
        const $widget = $(this).closest("[data-datetime-picker]");

        if (validateDateTimePicker($widget)) {
            closeDateTimePicker($widget);
        }
    });

    $(document).on("blur", "[data-datetime-display]", function () {
        const $widget = $(this).closest("[data-datetime-picker]");

        setTimeout(function () {
            if (!$widget.find(":focus").length) {
                validateDateTimePicker($widget);
                closeDateTimePicker($widget);
            }
        }, 150);
    });

    $(document).on("click", function (event) {
        const $target = $(event.target);

        $("[data-datetime-picker]").each(function () {
            const $widget = $(this);

            if (!$target.closest($widget[0]).length) {
                closeDateTimePicker($widget);
            }
        });
    });

    // Purchase order status and inventory stock badge animations.
    function animateBadge($badge) {
        $badge.removeClass("wms-badge-pop");
        void $badge[0].offsetWidth;
        $badge.addClass("wms-badge-pop");

        setTimeout(function () {
            $badge.removeClass("wms-badge-pop");
        }, 800);
    }

    function statusClass(status) {
        switch ((status || "").toLowerCase()) {
            case "approved":
                return "wms-badge-primary";
            case "shipped":
                return "wms-badge-info";
            case "delivered":
                return "wms-badge-success";
            case "cancelled":
                return "wms-badge-danger";
            default:
                return "wms-badge-warning";
        }
    }

    function updateStatusPreview($select) {
        const status = $.trim($select.find("option:selected").text());
        const $badge = $select.closest("form").find("[data-status-preview]");

        if (!$badge.length) {
            return;
        }

        $badge
            .removeClass("wms-badge-primary wms-badge-info wms-badge-success wms-badge-danger wms-badge-warning")
            .addClass(statusClass(status))
            .text(status);

        animateBadge($badge);
    }

    function stockState(quantity) {
        if (quantity <= 0) {
            return { text: "OUT OF STOCK", cssClass: "wms-badge-danger" };
        }

        if (quantity <= 10) {
            return { text: "LOW STOCK", cssClass: "wms-badge-warning" };
        }

        return { text: "IN STOCK", cssClass: "wms-badge-success" };
    }

    function updateStockPreview($input) {
        const quantity = parseInt($input.val(), 10) || 0;
        const state = stockState(quantity);
        const $form = $input.closest("form");
        const $badge = $form.find("[data-stock-preview]");
        const $units = $form.find("[data-stock-units]");

        if (!$badge.length) {
            return;
        }

        $badge
            .removeClass("wms-badge-success wms-badge-warning wms-badge-danger")
            .addClass(state.cssClass)
            .text(state.text);

        $units.text(quantity + " Units");
        animateBadge($badge);
    }

    $("[data-status-input]").each(function () {
        updateStatusPreview($(this));
    });

    $(document).on("change", "[data-status-input]", function () {
        updateStatusPreview($(this));
    });

    $("[data-stock-input]").each(function () {
        updateStockPreview($(this));
    });

    $(document).on("input change", "[data-stock-input]", function () {
        updateStockPreview($(this));
    });

    // Toast notifications after create, edit and delete actions.
    $("[data-wms-toast]").each(function () {
        const $toast = $(this);

        setTimeout(function () {
            $toast.addClass("wms-toast-visible");
        }, 80);

        setTimeout(function () {
            $toast.removeClass("wms-toast-visible");
        }, 4200);
    });

    $(document).on("click", "[data-toast-close]", function () {
        $(this).closest("[data-wms-toast]").removeClass("wms-toast-visible");
    });
});

// AI form assistant: sends a natural-language prompt to the server and fills the active Create form.
$(document).on("click", "[data-ai-generate]", function () {
    const entity = $(this).data("entity");
    const prompt = $("[data-ai-prompt='" + entity + "']").val();
    const token = $("input[name='__RequestVerificationToken']").first().val();
    const $message = $("[data-ai-message='" + entity + "']");

    if (!prompt || !prompt.trim()) {
        $message.html('<div class="wms-alert wms-alert-danger">Please enter a description.</div>');
        return;
    }

    $message.html('<div class="wms-alert wms-alert-info">Generating suggestion...</div>');

    $.ajax({
        url: "/ai/suggest",
        type: "POST",
        contentType: "application/json",
        headers: {
            RequestVerificationToken: token
        },
        data: JSON.stringify({
            entity: entity,
            prompt: prompt
        })
    }).done(function (response) {
        if (!response.success) {
            $message.html('<div class="wms-alert wms-alert-danger">' + response.message + '</div>');
            return;
        }

        fillFormFromAi(entity, response.data);

        $message.html('<div class="wms-alert wms-alert-success">' + response.message + '</div>');
    }).fail(function () {
        $message.html('<div class="wms-alert wms-alert-danger">AI suggestion failed. Please try again.</div>');
    });
});

// Select the correct form filler based on the entity returned from the AI assistant.
function fillFormFromAi(entity, data) {
    switch (entity) {
        case "category":
            fillCategoryForm(data);
            break;
        case "warehouse":
            fillWarehouseForm(data);
            break;
        case "supplier":
            fillSupplierForm(data);
            break;
        case "product":
            fillProductForm(data);
            break;
        case "location":
            fillLocationForm(data);
            break;
        case "inventory":
            fillInventoryForm(data);
            break;
        case "purchaseorder":
            fillPurchaseOrderForm(data);
            break;
        case "purchaseorderitem":
            fillPurchaseOrderItemForm(data);
            break;
    }
}

// Category Create form fields.
function fillCategoryForm(data) {
    setField("#Name", data.name);
    setField("#Description", data.description);
}

// Warehouse Create form fields.
function fillWarehouseForm(data) {
    setField("#Name", data.name);
    setField("#Address", data.address);
    setField("#City", data.city);
    setField("#Country", data.country);
    setField("#Capacity", data.capacity);
}

// Supplier Create form fields.
function fillSupplierForm(data) {
    setField("#Name", data.name);
    setField("#ContactPerson", data.contactPerson || data.contactName);
    setField("#ContactEmail", data.contactEmail);
    setField("#ContactPhone", data.contactPhone);
    setField("#ContactAddress", data.contactAddress);
}

// Product Create form fields.
function fillProductForm(data) {
    setField("#Name", data.name);
    setField("#Description", data.description);
    setField("#Price", data.price);
    setField("#Weight", data.weight);
    setDateTimeField("ProductReceivedAt", data.productReceivedAt);
    setAutocompleteField("CategoryId", data.categoryId, data.categoryName);
}

// Location Create form fields.
function fillLocationForm(data) {
    setField("#Code", data.code);
    setField("#Zone", data.zone);
    setField("#ShelfNumber", data.shelfNumber);
    setAutocompleteField("WarehouseId", data.warehouseId, data.warehouseName);
}

// Inventory Create form fields.
function fillInventoryForm(data) {
    setField("#Quantity", data.quantity);
    setDateTimeField("LastUpdated", data.lastUpdated);
    setAutocompleteField("ProductId", data.productId, data.productName);
    setAutocompleteField("LocationId", data.locationId, data.locationCode);
}

// PurchaseOrder Create form fields.
function fillPurchaseOrderForm(data) {
    setField("#TotalAmount", data.totalAmount);
    setAutocompleteField("SupplierId", data.supplierId, data.supplierName);
    setAutocompleteField("WarehouseId", data.warehouseId, data.warehouseName);
    setSelectByTextOrValue("#Status", data.status);
    setDateTimeField("OrderDate", data.orderDate);
    setDateTimeField("ExpectedDeliveryDate", data.expectedDeliveryDate);
}

// PurchaseOrderItem Create form fields.
function fillPurchaseOrderItemForm(data) {
    setAutocompleteField("PurchaseOrderId", data.purchaseOrderId, formatPurchaseOrderText(data.purchaseOrderNumber));
    setAutocompleteField("ProductId", data.productId, data.productName);
    setField("#Quantity", data.quantity);
    setField("#UnitPrice", data.unitPrice);
}

// Fill a normal input/select/textarea and trigger validation refresh.
function setField(selector, value) {
    if (value === null || value === undefined) {
        return;
    }

    $(selector)
        .val(value)
        .trigger("change")
        .trigger("keyup");
}

// Select an option by value first, then by visible option text.
function setSelectByTextOrValue(selector, value) {
    if (value === null || value === undefined) {
        return;
    }

    const $select = $(selector);
    const normalizedValue = value.toString().trim().toLowerCase();
    let matchedValue = null;

    $select.find("option").each(function () {
        const optionValue = $(this).val().toString().trim().toLowerCase();
        const optionText = $(this).text().trim().toLowerCase();

        if (optionValue === normalizedValue || optionText === normalizedValue) {
            matchedValue = $(this).val();
            return false;
        }
    });

    if (matchedValue !== null) {
        $select
            .val(matchedValue)
            .trigger("change")
            .trigger("keyup");
    }
}

// Keep purchase order autocomplete text consistent for values like "1001" and "PO-1001".
function formatPurchaseOrderText(orderNumber) {
    if (!orderNumber) {
        return null;
    }

    const text = orderNumber.toString().trim();
    return text.toLowerCase().startsWith("po-") ? text : "PO-" + text;
}

// Fill the custom AJAX autocomplete dropdown by setting both hidden ID and visible text.
function setAutocompleteField(fieldName, id, text) {
    if (!id) {
        return;
    }

    const $value = $("[data-autocomplete-value][name='" + fieldName + "']");
    const $widget = $value.closest("[data-autocomplete]");

    if (!$value.length || !$widget.length) {
        return;
    }

    $value.val(id).trigger("change");
    $widget.find("[data-autocomplete-input]").val(text || "");
    $widget.find("[data-autocomplete-error]")
        .removeClass("field-validation-error")
        .addClass("field-validation-valid")
        .text("");

    $widget.removeClass("wms-autocomplete-invalid");
}

// Fill the custom date-time partial view control.
function setDateTimeField(fieldName, value) {
    if (!value) {
        return;
    }

    const $value = $("[data-datetime-value][name='" + fieldName + "']");
    const $widget = $value.closest("[data-datetime-picker]");

    if (!$value.length || !$widget.length) {
        return;
    }

    $value.val(value).trigger("change");
    $widget.find("[data-datetime-display]").val(value).trigger("blur");
}

// Global search for pages, actions and application data.
const $globalSearchInput = $("#globalSearchInput");
const $globalSearchResults = $("#globalSearchResults");

function renderGlobalSearchResults(results) {
    $globalSearchResults.empty();

    if (!results.length) {
        $("<div>")
            .addClass("wms-global-search-empty")
            .text("No results found.")
            .appendTo($globalSearchResults);

        $globalSearchResults.prop("hidden", false);
        return;
    }

    results.forEach(function (result) {
        const $item = $("<a>")
            .addClass("wms-global-search-item")
            .attr("href", result.url);

        $("<span>")
            .addClass("wms-global-search-type")
            .text(result.type)
            .appendTo($item);

        $("<strong>")
            .text(result.title)
            .appendTo($item);

        $("<small>")
            .text(result.subtitle)
            .appendTo($item);

        $item.appendTo($globalSearchResults);
    });

    $globalSearchResults.prop("hidden", false);
}

function runGlobalSearch() {
    const term = $.trim($globalSearchInput.val());

    if (term.length < 2) {
        $globalSearchResults.empty().prop("hidden", true);
        return;
    }

    $.get("/global-search", { term: term })
        .done(function (results) {
            renderGlobalSearchResults(results);
        })
        .fail(function () {
            $globalSearchResults
                .empty()
                .append(
                    $("<div>")
                        .addClass("wms-global-search-empty")
                        .text("Search failed.")
                )
                .prop("hidden", false);
        });
}

$globalSearchInput.on("input", function () {
    clearTimeout($globalSearchInput.data("search-timer"));

    $globalSearchInput.data("search-timer", setTimeout(function () {
        runGlobalSearch();
    }, 250));
});

$(document).on("click", function (event) {
    if (!$(event.target).closest(".wms-global-search").length) {
        $globalSearchResults.prop("hidden", true);
    }
});

$globalSearchInput.on("focus", function () {
    if ($globalSearchResults.children().length) {
        $globalSearchResults.prop("hidden", false);
    }
});

// Keep the mobile navigation in its off-canvas state from the first Bootstrap
// collapse frame, and let a click on the dimmed page close it.
const mobileNav = document.getElementById("wmsMobileNav");
const mobileSidebar = document.querySelector(".wms-sidebar");
const mainContent = document.querySelector(".wms-main-content");
const mobileQuery = window.matchMedia("(max-width: 767.98px)");

function setMobileNavOpen(isOpen) {
    if (!mobileSidebar || !mobileQuery.matches) {
        return;
    }

    mobileSidebar.classList.toggle("is-mobile-nav-open", isOpen);
    document.body.classList.toggle("wms-mobile-nav-open", isOpen);
}

if (mobileNav && mobileSidebar) {
    mobileNav.addEventListener("show.bs.collapse", function () {
        setMobileNavOpen(true);
    });

    mobileNav.addEventListener("hidden.bs.collapse", function () {
        setMobileNavOpen(false);
    });

    mobileQuery.addEventListener("change", function (event) {
        if (!event.matches) {
            mobileSidebar.classList.remove("is-mobile-nav-open");
            document.body.classList.remove("wms-mobile-nav-open");
        }
    });

    mainContent?.addEventListener("click", function () {
        if (mobileQuery.matches && mobileSidebar.classList.contains("is-mobile-nav-open")) {
            bootstrap.Collapse.getOrCreateInstance(mobileNav).hide();
        }
    });
}
