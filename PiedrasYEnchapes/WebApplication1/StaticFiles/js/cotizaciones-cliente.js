document.addEventListener("DOMContentLoaded", function () {
    const cards = document.querySelectorAll(".reveal-card");

    if ("IntersectionObserver" in window) {
        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach((entry, index) => {
                if (entry.isIntersecting) {
                    setTimeout(() => {
                        entry.target.classList.add("is-visible");
                    }, index * 80);
                    obs.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.15
        });

        cards.forEach(card => observer.observe(card));
    } else {
        cards.forEach(card => card.classList.add("is-visible"));
    }
});