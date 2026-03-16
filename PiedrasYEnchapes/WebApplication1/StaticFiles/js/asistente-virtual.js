document.addEventListener("DOMContentLoaded", function () {
    const assistantToggle = document.getElementById("assistantToggle");
    const assistantClose = document.getElementById("assistantClose");
    const assistantPanel = document.getElementById("assistantPanel");
    const assistantBubble = document.getElementById("assistantBubble");
    const assistantMessages = document.getElementById("assistantMessages");
    const assistantInput = document.getElementById("assistantInput");
    const assistantSend = document.getElementById("assistantSend");
    const quickButtons = document.querySelectorAll(".quick-question");

    function toggleAssistant() {
        assistantPanel.classList.toggle("open");

        if (assistantPanel.classList.contains("open")) {
            assistantBubble.style.display = "none";
            assistantInput.focus();
        } else {
            assistantBubble.style.display = "block";
        }
    }

    function addMessage(text, sender) {
        const message = document.createElement("div");
        message.className = `assistant-message ${sender}`;
        message.textContent = text;
        assistantMessages.appendChild(message);
        assistantMessages.scrollTop = assistantMessages.scrollHeight;
    }

    function getFakeResponse(question) {
        const q = question.toLowerCase();

        if (q.includes("producto") || q.includes("productos") || q.includes("piedra") || q.includes("enchape")) {
            return "Podés ver nuestros productos disponibles en la sección de catálogo. Ahí encontrarás piedras decorativas, enchapes y opciones para distintos estilos de proyecto.";
        }

        if (q.includes("cotizar") || q.includes("cotización") || q.includes("cotizacion")) {
            return "Para cotizar, solo debés ingresar a la sección de cotizaciones, seleccionar los productos de tu interés y completar la solicitud. Luego podrás dar seguimiento al estado de tu cotización.";
        }

        if (q.includes("precio") || q.includes("precios") || q.includes("costo")) {
            return "Los precios pueden variar según el producto, la cantidad y la disponibilidad. Te recomiendo revisar el catálogo o generar una cotización para obtener un estimado más preciso.";
        }

        if (q.includes("envio") || q.includes("envíos") || q.includes("entrega")) {
            return "La información sobre entrega puede variar según la zona y el volumen solicitado. Para conocer más detalles, lo ideal es realizar una cotización o comunicarte con el negocio.";
        }

        if (q.includes("pago") || q.includes("pagos") || q.includes("método") || q.includes("metodo")) {
            return "Los métodos de pago pueden confirmarse directamente durante el proceso de atención. Si deseas, puedes avanzar con tu cotización para recibir información más específica.";
        }

        if (q.includes("horario") || q.includes("horarios") || q.includes("hora")) {
            return "Puedes consultar los horarios de atención desde la sección de contacto o comunicarte directamente con el negocio para una respuesta más precisa.";
        }

        if (q.includes("contacto") || q.includes("telefono") || q.includes("whatsapp")) {
            return "En la sección de contacto podrás encontrar los medios disponibles para comunicarte con la empresa y recibir atención personalizada.";
        }

        if (q.includes("hola") || q.includes("buenas") || q.includes("buenos días") || q.includes("buenas tardes")) {
            return "¡Hola! Con gusto puedo orientarte sobre productos, cotizaciones, contacto y consultas frecuentes.";
        }

        return "Puedo ayudarte con preguntas sobre productos, cotizaciones, pagos, envíos, horarios y contacto. También puedes usar los botones rápidos para navegar más fácil.";
    }

    function handleQuestion(question) {
        if (!question || !question.trim()) return;

        addMessage(question, "user");

        setTimeout(() => {
            const response = getFakeResponse(question);
            addMessage(response, "bot");
        }, 500);
    }

    if (assistantToggle) {
        assistantToggle.addEventListener("click", toggleAssistant);
    }

    if (assistantClose) {
        assistantClose.addEventListener("click", toggleAssistant);
    }

    if (assistantSend) {
        assistantSend.addEventListener("click", function () {
            const question = assistantInput.value.trim();
            if (question) {
                handleQuestion(question);
                assistantInput.value = "";
            }
        });
    }

    if (assistantInput) {
        assistantInput.addEventListener("keypress", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                const question = assistantInput.value.trim();
                if (question) {
                    handleQuestion(question);
                    assistantInput.value = "";
                }
            }
        });
    }

    quickButtons.forEach(button => {
        button.addEventListener("click", function () {
            const question = this.getAttribute("data-question");
            handleQuestion(question);
        });
    });
});