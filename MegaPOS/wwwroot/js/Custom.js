window.Leaderboard = {
    changeOrder: function () {
        const allSlides = document.querySelectorAll(".single-slide");
        const previous = "1";
        const current = "2";
        const next = "3";

        for (const slide of allSlides) {
            const order = slide.getAttribute("data-order");

            switch (order) {
                case current:
                    slide.setAttribute("data-order", previous);
                    break;
                case next:
                    slide.setAttribute("data-order", current);
                    break;
                case previous:
                    slide.setAttribute("data-order", next);
                    break;
            }
        }
    },
    animateValue: function (elementId, start, end, duration) {
        let obj = document.getElementById(elementId);
        let startTimestamp = null;
        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            obj.innerHTML = ((progress * (end - start)) + start).toFixed(2);
            if (progress < 1) {
                window.requestAnimationFrame(step);
            }
        };
        window.requestAnimationFrame(step);
    },
    animateScroll: function (elementid) {
        let obj = document.getElementById(elementid);
        obj.scrollTo({ top: obj.scrollHeight, behavior: 'smooth' });
        setTimeout(function () {
            obj.scrollTo({ top: 0, behavior: 'smooth' });
        }, 4000);
        setInterval(function () {
            // 4000 - it will take 4 secound in total from the top of the page to the bottom
            obj.scrollTo({ top: obj.scrollHeight, behavior: 'smooth' });
            setTimeout(function () {
                obj.scrollTo({ top: 0, behavior: 'smooth' });
            }, 4000);
        }, 8000);
    }

}