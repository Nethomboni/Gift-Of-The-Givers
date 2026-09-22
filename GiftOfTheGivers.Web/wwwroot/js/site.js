// Gift of the Givers - prototype UI enhancements.
// Progressive enhancement only: every form here already works via a normal
// POST and server-side validation (Section 47/48) if JavaScript is off.
(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", function () {
    initDonationAmountButtons();
    initDonationTypeToggle();
    initAnonymousToggle();
    initPasswordToggles();
    initAnimatedCounters();
    initDonationReviewSync();
  });

  // ---- Donation: preset amount buttons -----------------------------------
  function initDonationAmountButtons() {
    var buttons = document.querySelectorAll("[data-amount-btn]");
    var amountInput = document.getElementById("Amount");
    if (!buttons.length || !amountInput) return;

    function clearSelection() {
      buttons.forEach(function (b) { b.classList.remove("selected"); });
    }

    buttons.forEach(function (btn) {
      btn.addEventListener("click", function () {
        clearSelection();
        btn.classList.add("selected");
        amountInput.value = btn.getAttribute("data-amount-btn");
        amountInput.dispatchEvent(new Event("input"));
      });
    });

    // Typing a custom amount deselects the preset buttons.
    amountInput.addEventListener("input", function () {
      var current = amountInput.value;
      var matched = false;
      buttons.forEach(function (b) {
        if (b.getAttribute("data-amount-btn") === current) {
          b.classList.add("selected");
          matched = true;
        } else {
          b.classList.remove("selected");
        }
      });
    });
  }

  // ---- Donation: One-Time vs Recurring -----------------------------------
  function initDonationTypeToggle() {
    var radios = document.querySelectorAll('input[name="DonationType"]');
    var frequencyWrap = document.getElementById("frequency-wrap");
    if (!radios.length || !frequencyWrap) return;

    function sync() {
      var selected = document.querySelector('input[name="DonationType"]:checked');
      var isRecurring = selected && selected.value === "Recurring";
      frequencyWrap.classList.toggle("d-none", !isRecurring);
    }

    radios.forEach(function (r) { r.addEventListener("change", sync); });
    sync();
  }

  // ---- Donation: Anonymous guest toggle ----------------------------------
  function initAnonymousToggle() {
    var checkbox = document.getElementById("IsAnonymous");
    var nameWrap = document.getElementById("donor-name-wrap");
    if (!checkbox || !nameWrap) return;

    function sync() {
      nameWrap.classList.toggle("d-none", checkbox.checked);
      var input = nameWrap.querySelector("input");
      if (input) {
        input.disabled = checkbox.checked;
      }
    }

    checkbox.addEventListener("change", sync);
    sync();
  }

  // ---- Live donation review summary --------------------------------------
  function initDonationReviewSync() {
    var form = document.getElementById("donation-form");
    if (!form) return;

    var reviewAmount = document.getElementById("review-amount");
    var reviewCurrency = document.getElementById("review-currency");
    var reviewCause = document.getElementById("review-cause");
    var reviewType = document.getElementById("review-type");

    var amountInput = document.getElementById("Amount");
    var currencySelect = document.getElementById("Currency");
    var causeSelect = document.getElementById("Cause");
    var typeRadios = document.querySelectorAll('input[name="DonationType"]');
    var freqSelect = document.getElementById("Frequency");

    function update() {
      if (reviewAmount && amountInput) {
        var amt = parseFloat(amountInput.value);
        reviewAmount.textContent = isNaN(amt) ? "-" : amt.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
      }
      if (reviewCurrency && currencySelect) {
        reviewCurrency.textContent = currencySelect.value;
      }
      if (reviewCause && causeSelect) {
        reviewCause.textContent = causeSelect.value;
      }
      if (reviewType) {
        var selected = document.querySelector('input[name="DonationType"]:checked');
        var typeLabel = selected && selected.value === "Recurring" ? "Recurring" : "One-Time";
        if (typeLabel === "Recurring" && freqSelect) {
          typeLabel += " (" + freqSelect.value + ")";
        }
        reviewType.textContent = typeLabel;
      }
    }

    [amountInput, currencySelect, causeSelect, freqSelect].forEach(function (el) {
      if (el) { el.addEventListener("input", update); el.addEventListener("change", update); }
    });
    typeRadios.forEach(function (r) { r.addEventListener("change", update); });

    update();
  }

  // ---- Password show/hide --------------------------------------------------
  function initPasswordToggles() {
    document.querySelectorAll("[data-toggle-password]").forEach(function (btn) {
      var targetId = btn.getAttribute("data-toggle-password");
      var input = document.getElementById(targetId);
      if (!input) return;

      btn.addEventListener("click", function () {
        var showing = input.type === "text";
        input.type = showing ? "password" : "text";
        var icon = btn.querySelector("i");
        if (icon) {
          icon.classList.toggle("bi-eye", showing);
          icon.classList.toggle("bi-eye-slash", !showing);
        }
        btn.setAttribute("aria-label", showing ? "Show password" : "Hide password");
      });
    });
  }

  // ---- Animated statistic counters ----------------------------------------
  function initAnimatedCounters() {
    var counters = document.querySelectorAll("[data-count-to]");
    if (!counters.length) return;

    var prefersReducedMotion = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    function animate(el) {
      var target = parseFloat(el.getAttribute("data-count-to"));
      if (isNaN(target)) return;

      if (prefersReducedMotion) {
        el.textContent = formatCount(target, el);
        return;
      }

      var duration = 1200;
      var start = null;

      function step(timestamp) {
        if (!start) start = timestamp;
        var progress = Math.min((timestamp - start) / duration, 1);
        var eased = 1 - Math.pow(1 - progress, 3);
        el.textContent = formatCount(target * eased, el);
        if (progress < 1) {
          window.requestAnimationFrame(step);
        } else {
          el.textContent = formatCount(target, el);
        }
      }
      window.requestAnimationFrame(step);
    }

    function formatCount(value, el) {
      var suffix = el.getAttribute("data-suffix") || "";
      return Math.round(value).toLocaleString() + suffix;
    }

    if ("IntersectionObserver" in window) {
      var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            animate(entry.target);
            observer.unobserve(entry.target);
          }
        });
      }, { threshold: 0.4 });

      counters.forEach(function (el) { observer.observe(el); });
    } else {
      counters.forEach(animate);
    }
  }
})();
