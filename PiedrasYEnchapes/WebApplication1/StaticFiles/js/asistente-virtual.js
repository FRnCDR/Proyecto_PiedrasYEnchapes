document.addEventListener("DOMContentLoaded", function () {
    const assistantToggle = document.getElementById("assistantToggle");
    const assistantClose = document.getElementById("assistantClose");
    const assistantPanel = document.getElementById("assistantPanel");
    const assistantBubble = document.getElementById("assistantBubble");
    const assistantMessages = document.getElementById("assistantMessages");
    const assistantInput = document.getElementById("assistantInput");
    const assistantSend = document.getElementById("assistantSend");
    const quickButtons = document.querySelectorAll(".quick-question");

    const config = window.assistantConfig || {};
    const routes = config.routes || {};
    const contacto = config.contacto || {};
    const userName = config.userName || "amigo";

    function normalizar(texto) {
        return texto
            .toLowerCase()
            .normalize("NFD")
            .replace(/[̀-ͯ]/g, "");
    }

    // Cada intención define palabras clave (ya normalizadas, sin acentos) y cómo responder.
    // El orden importa como desempate: ante igual cantidad de coincidencias, gana la primera.
    const intents = [
        {
            id: "saludo",
            keywords: ["hola", "buenas", "buenos dias", "buenas tardes", "buenas noches", "que tal", "hey"],
            respond: () => ({
                text: `¡Hola ${userName}! Soy Lito. Puedo ayudarte con productos, cotizaciones, tu carrito, pagos, envíos, horarios o contacto.`
            })
        },
        {
            id: "productos",
            keywords: ["producto", "productos", "piedra", "piedras", "enchape", "enchapes", "catalogo", "ver productos"],
            respond: () => ({
                text: `Claro ${userName}, este es nuestro catálogo de piedras decorativas y enchapes. Podés filtrar y ver el detalle de cada producto ahí mismo:`,
                actions: routes.productos ? [{ label: "Ir a productos", url: routes.productos }] : []
            })
        },
        {
            id: "carrito",
            keywords: ["carrito", "mi carrito", "lo que agregue", "lo que añadi"],
            respond: () => ({
                text: "Podés revisar y ajustar lo que llevás en tu carrito acá:",
                actions: routes.carrito ? [{ label: "Ver mi carrito", url: routes.carrito }] : []
            })
        },
        {
            id: "cotizar",
            keywords: ["cotizar", "cotizacion", "presupuesto", "cuanto cuesta un proyecto"],
            respond: () => ({
                text: "Para cotizar, seleccioná los productos de tu interés y completá la solicitud desde esta sección:",
                actions: routes.cotizar ? [{ label: "Crear cotización", url: routes.cotizar }] : []
            })
        },
        {
            id: "misCotizaciones",
            keywords: ["mis cotizaciones", "estado de mi cotizacion", "seguimiento de cotizacion", "donde veo mi cotizacion"],
            respond: () => ({
                text: "Podés dar seguimiento al estado de tus cotizaciones desde acá:",
                actions: routes.misCotizaciones ? [{ label: "Ver mis cotizaciones", url: routes.misCotizaciones }] : []
            })
        },
        {
            id: "historial",
            keywords: ["historial", "mis compras", "compras anteriores", "pedidos anteriores"],
            respond: () => ({
                text: "Tu historial de compras está disponible en esta sección:",
                actions: routes.historial ? [{ label: "Ver historial de compras", url: routes.historial }] : []
            })
        },
        {
            id: "precio",
            keywords: ["precio", "precios", "costo", "cuanto vale", "cuanto cuesta"],
            respond: () => ({
                text: "Los precios varían según el producto, la cantidad y la disponibilidad. Lo más preciso es generar una cotización:",
                actions: routes.cotizar ? [{ label: "Crear cotización", url: routes.cotizar }] : []
            })
        },
        {
            id: "envio",
            keywords: ["envio", "envios", "entrega", "despacho"],
            respond: () => ({
                text: "La entrega puede variar según la zona y el volumen solicitado. Contanos tu caso y te confirmamos los detalles:",
                actions: contacto.telHref ? [{ label: `Llamar al ${contacto.telefono}`, url: contacto.telHref }] : []
            })
        },
        {
            id: "pago",
            keywords: ["pago", "pagos", "metodo de pago", "metodos de pago", "sinpe", "tarjeta"],
            respond: () => ({
                text: "Los métodos de pago se confirman directamente en la atención. Si querés, avanzá con tu cotización o escribinos:",
                actions: [
                    ...(routes.cotizar ? [{ label: "Crear cotización", url: routes.cotizar }] : []),
                    ...(contacto.telHref ? [{ label: "Contactar", url: contacto.telHref }] : [])
                ]
            })
        },
        {
            id: "horario",
            keywords: ["horario", "horarios", "a que hora", "hora de atencion"],
            respond: () => ({
                text: contacto.horario
                    ? `Nuestro horario de atención es: ${contacto.horario}.`
                    : "Podés consultar el horario de atención en la sección de contacto."
            })
        },
        {
            id: "contacto",
            keywords: ["contacto", "telefono", "whatsapp", "correo", "email", "comunicarme"],
            respond: () => ({
                text: `Podés escribirnos a ${contacto.email || "nuestro correo"} o llamarnos al ${contacto.telefono || "nuestro teléfono"}.`,
                actions: [
                    ...(contacto.telHref ? [{ label: "Llamar", url: contacto.telHref }] : []),
                    ...(contacto.emailHref ? [{ label: "Enviar correo", url: contacto.emailHref }] : [])
                ]
            })
        },
        {
            id: "nosotros",
            keywords: ["quienes son", "sobre ustedes", "sobre nosotros", "quienes somos", "la empresa"],
            respond: () => ({
                text: "Te contamos quiénes somos y cómo trabajamos en esta sección:",
                actions: routes.nosotros ? [{ label: "Sobre nosotros", url: routes.nosotros }] : []
            })
        }
    ];

    function matchIntent(question) {
        const q = normalizar(question);
        let best = null;
        let bestScore = 0;

        for (const intent of intents) {
            const score = intent.keywords.reduce((total, kw) => total + (q.includes(kw) ? 1 : 0), 0);
            if (score > bestScore) {
                bestScore = score;
                best = intent;
            }
        }

        return best;
    }

    function getAssistantResponse(question) {
        const intent = matchIntent(question);

        if (intent) {
            return intent.respond();
        }

        return {
            text: "Puedo ayudarte con preguntas sobre productos, cotizaciones, tu carrito, pagos, envíos, horarios y contacto. También podés usar los botones rápidos para navegar más fácil."
        };
    }

    function toggleAssistant() {
        assistantPanel.classList.toggle("open");

        if (assistantPanel.classList.contains("open")) {
            assistantBubble.style.display = "none";
            assistantInput.focus();
        } else {
            assistantBubble.style.display = "block";
        }
    }

    function addMessage(text, sender, actions) {
        const message = document.createElement("div");
        message.className = `assistant-message ${sender}`;

        const textEl = document.createElement("div");
        textEl.textContent = text;
        message.appendChild(textEl);

        if (actions && actions.length) {
            const actionsWrap = document.createElement("div");
            actionsWrap.className = "assistant-message-actions";

            actions.forEach(action => {
                const link = document.createElement("a");
                link.href = action.url;
                link.className = "assistant-action-btn";
                link.textContent = action.label;
                actionsWrap.appendChild(link);
            });

            message.appendChild(actionsWrap);
        }

        assistantMessages.appendChild(message);
        assistantMessages.scrollTop = assistantMessages.scrollHeight;
    }

    function handleQuestion(question) {
        if (!question || !question.trim()) return;

        addMessage(question, "user");

        setTimeout(() => {
            const response = getAssistantResponse(question);
            addMessage(response.text, "bot", response.actions);
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
