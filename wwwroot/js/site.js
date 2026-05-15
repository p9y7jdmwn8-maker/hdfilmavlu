document.addEventListener("click", (event) => {
    const carouselButton = event.target.closest(".carousel-arrow");
    if (carouselButton) {
        const section = carouselButton.closest(".movie-section");
        const row = section?.querySelector(".movie-row");
        if (row) {
            const direction = Number(carouselButton.dataset.direction || "1");
            row.scrollBy({
                left: direction * Math.max(220, row.clientWidth * 0.72),
                behavior: "smooth"
            });
        }
        return;
    }

    const heroDot = event.target.closest("[data-hero-slide]");
    if (heroDot) {
        setHeroSlide(heroDot);
        return;
    }

    const button = event.target.closest("[data-favorite]");
    if (!button) {
        return;
    }

    const key = "hdfilmavlu:favorites";
    const id = button.getAttribute("data-favorite");
    const favorites = new Set(JSON.parse(localStorage.getItem(key) || "[]"));

    if (favorites.has(id)) {
        favorites.delete(id);
        button.textContent = "+ LİSTEYE EKLE";
    } else {
        favorites.add(id);
        button.textContent = "✓ LİSTEDE";
    }

    localStorage.setItem(key, JSON.stringify([...favorites]));
});

function setHeroSlide(heroDot) {
    const hero = heroDot.closest(".filmio-hero");
    const dots = hero?.querySelectorAll("[data-hero-slide]");
    dots?.forEach((dot) => dot.classList.toggle("active", dot === heroDot));
    hero?.setAttribute("data-slide", heroDot.dataset.heroSlide || "0");

    const title = hero?.querySelector("[data-hero-title]");
    const description = hero?.querySelector("[data-hero-description]");
    const tag = hero?.querySelector("[data-hero-tag]");

    if (title && heroDot.dataset.title) {
        title.textContent = heroDot.dataset.title;
    }

    if (description && heroDot.dataset.description) {
        description.textContent = heroDot.dataset.description;
    }

    if (tag && heroDot.dataset.tag) {
        tag.textContent = heroDot.dataset.tag;
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const hero = document.querySelector(".filmio-hero");
    const dots = [...document.querySelectorAll("[data-hero-slide]")];
    if (!hero || dots.length < 2) {
        return;
    }

    let index = 0;
    window.setInterval(() => {
        index = (index + 1) % dots.length;
        setHeroSlide(dots[index]);
    }, 5500);
});
