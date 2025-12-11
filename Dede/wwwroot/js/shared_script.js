// wwwroot/js/shared_script.js

// Модальные окна
const modals = {
    login: document.getElementById('loginModal'),
    register: document.getElementById('registerModal'),
    contact: document.getElementById('contactModal'),
    service: document.getElementById('serviceModal'),
    order: document.getElementById('orderModal')
};

// Бургер-меню
const burgerBtn = document.querySelector('.burger-btn');
const mainNav = document.querySelector('.main-nav');

if (burgerBtn) {
    burgerBtn.addEventListener('click', () => {
        burgerBtn.classList.toggle('active');
        mainNav.classList.toggle('active');
        document.body.style.overflow = mainNav.classList.contains('active') ? 'hidden' : '';
    });

    document.querySelectorAll('.nav-menu a').forEach(link => {
        link.addEventListener('click', () => {
            burgerBtn.classList.remove('active');
            mainNav.classList.remove('active');
            document.body.style.overflow = '';
        });
    });

    window.addEventListener('resize', () => {
        if (window.innerWidth > 968) {
            burgerBtn.classList.remove('active');
            mainNav.classList.remove('active');
            document.body.style.overflow = '';
        }
    });
}

// Открытие модального окна
document.querySelectorAll('[data-modal]').forEach(btn => {
    btn.addEventListener('click', () => {
        const modalType = btn.dataset.modal;
        if (modals[modalType]) {
            modals[modalType].classList.add('active');
            document.body.style.overflow = 'hidden';
        }
    });
});

// Закрытие модального окна
function closeAllModals() {
    Object.values(modals).forEach(modal => {
        modal.classList.remove('active');
    });
    document.body.style.overflow = '';
}

document.querySelectorAll('[data-close-modal]').forEach(el => {
    el.addEventListener('click', closeAllModals);
});

// Переключение между модальными окнами
document.querySelectorAll('[data-switch-modal]').forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        const targetModal = link.dataset.switchModal;

        Object.values(modals).forEach(modal => {
            modal.classList.remove('active');
        });

        if (modals[targetModal]) {
            modals[targetModal].classList.add('active');
        }
    });
});

// Закрытие по ESC
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        closeAllModals();

        if (burgerBtn && mainNav) {
            burgerBtn.classList.remove('active');
            mainNav.classList.remove('active');
        }
    }
});

// Плавная прокрутка к якорям
// Плавная прокрутка / переход на главную
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const targetId = this.getAttribute('href');
        if (!targetId || targetId === '#') return;

        const homePath = new URL(HOME_URL, window.location.origin).pathname.toLowerCase();
        const currentPath = window.location.pathname.toLowerCase();

        // Если мы не на главной странице — сначала переходим на неё
        if (currentPath !== homePath) {
            e.preventDefault();
            window.location.href = HOME_URL + targetId;
            return;
        }

        // Уже на главной: плавный скролл
        e.preventDefault();
        const target = document.querySelector(targetId);
        if (target) {
            const header = document.querySelector('header');
            const headerHeight = header ? header.offsetHeight : 0;
            const targetPosition = target.offsetTop - headerHeight - 20;

            window.scrollTo({
                top: targetPosition,
                behavior: 'smooth'
            });
        }
    });
});






// ==================== ФОРМЫ С FETCH ЗАПРОСАМИ ====================

// Форма входа
const loginForm = document.querySelector('#loginModal .modal-form');
if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const emailInput = loginForm.querySelector('input[type="email"]');
        const passwordInput = loginForm.querySelector('input[type="password"]');
        const submitBtn = loginForm.querySelector('button[type="submit"]');

        const email = emailInput.value.trim();
        const password = passwordInput.value.trim();

        // Валидация
        if (!email || !password) {
            FormUtils.showError(loginForm, 'Заполните все поля');
            return;
        }

        try {
            FormUtils.setButtonLoading(submitBtn, true);

            // Отправка запроса на сервер
            const response = await AuthAPI.login(email, password);

            FormUtils.showSuccess(loginForm, response.message || 'Вход выполнен успешно!');

            // Сохранение данных пользователя (если нужно)
            if (response.data) {
                localStorage.setItem('user', JSON.stringify(response.data));
            }

            // Закрытие модального окна через 1 секунду
            setTimeout(() => {
                closeAllModals();
                FormUtils.resetForm(loginForm);
                // Обновление UI для авторизованного пользователя
                updateAuthUI(response.data);
            }, 1000);

        } catch (error) {
            const message = error.message || 'Ошибка при входе';
            FormUtils.showError(loginForm, message);
        } finally {
            FormUtils.setButtonLoading(submitBtn, false);
        }
    });
}

// Форма регистрации
// Форма регистрации
const registerForm = document.querySelector('#registerModal .modal-form');
if (registerForm) {
    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const inputs = registerForm.querySelectorAll('input');
        const nameInput = inputs[0];
        const emailInput = inputs[1];
        const phoneInput = inputs[2];
        const passwordInput = inputs[3];
        const confirmPasswordInput = inputs[4];
        const submitBtn = registerForm.querySelector('button[type="submit"]');

        const name = nameInput.value.trim();
        const email = emailInput.value.trim();
        const phone = phoneInput.value.trim();
        const password = passwordInput.value.trim();
        const confirmPassword = confirmPasswordInput.value.trim();

        // Валидация
        if (!name || !email || !phone || !password || !confirmPassword) {
            FormUtils.showError(registerForm, 'Заполните все поля');
            return;
        }

        if (password !== confirmPassword) {
            FormUtils.showError(registerForm, 'Пароли не совпадают');
            return;
        }

        if (password.length < 6) {
            FormUtils.showError(registerForm, 'Пароль должен содержать минимум 6 символов');
            return;
        }

        try {
            FormUtils.setButtonLoading(submitBtn, true);

            const response = await AuthAPI.register(name, email, phone, password, confirmPassword);

            FormUtils.showSuccess(
                registerForm,
                response.message || 'Регистрация успешна! Проверьте почту для подтверждения.'
            );


            passwordInput.value = '';
            confirmPasswordInput.value = '';

        } catch (error) {
            const firstError =
                error.data?.errors?.[0]?.error ||
                error.message ||
                'Ошибка при регистрации';
            FormUtils.showError(registerForm, firstError);
        } finally {
            FormUtils.setButtonLoading(submitBtn, false);
        }
    });
}


// Форма обратной связи
const contactForm = document.getElementById('contactForm');
if (contactForm) {
    const nameInput = document.getElementById('contactName');
    const emailInput = document.getElementById('contactEmail');
    const subjectInput = document.getElementById('contactSubject');
    const messageInput = document.getElementById('contactMessage');
    const submitBtn = document.getElementById('contactSubmit');

    // Валидация формы
    const validateContactForm = () => {
        const isValid =
            nameInput.value.trim() !== '' &&
            emailInput.value.trim() !== '' &&
            subjectInput.value.trim() !== '' &&
            messageInput.value.trim() !== '' &&
            emailInput.validity.valid;

        submitBtn.disabled = !isValid;
    };

    [nameInput, emailInput, subjectInput, messageInput].forEach(input => {
        input.addEventListener('input', validateContactForm);
        input.addEventListener('blur', validateContactForm);
    });

    // Отправка формы
    contactForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const name = nameInput.value.trim();
        const email = emailInput.value.trim();
        const subject = subjectInput.value.trim();
        const message = messageInput.value.trim();

        try {
            FormUtils.setButtonLoading(submitBtn, true);

            // Отправка запроса на сервер
            const response = await ContactAPI.send(name, email, subject, message);

            FormUtils.showSuccess(contactForm, response.message || 'Сообщение отправлено!');

            console.log('Ответ сервера:', response);

            // Закрытие модального окна через 1.5 секунды
            setTimeout(() => {
                closeAllModals();
                FormUtils.resetForm(contactForm);
                validateContactForm();
            }, 1500);

        } catch (error) {
            const message = error.message || 'Ошибка при отправке сообщения';
            FormUtils.showError(contactForm, message);
        } finally {
            FormUtils.setButtonLoading(submitBtn, false);
        }
    });
}

// Обновление UI для авторизованного пользователя


// ============ УСЛУГИ И ОФОРМЛЕНИЕ ЗАКАЗА ============

// инициализация секции услуг (сортировка)
function initServicesSection() {
    const container = document.getElementById('servicesContainer');
    if (!container) return;

    const sortSelect = document.getElementById('servicesSort');

    renderServices();

    if (sortSelect) {
        sortSelect.addEventListener('change', () => {
            renderServices(sortSelect.value);
        });
    }
}

// Открытие модального окна заказа
function openOrderModal(service) {
    const modal = modals.order;
    if (!modal) return;

    const idInput = document.getElementById('orderServiceId');
    const infoDiv = document.getElementById('orderServiceInfo');

    const name = service.name || service.Name;
    const minPrice = service.minPrice ?? service.MinPrice;

    if (idInput) idInput.value = service.id || service.Id;
    if (infoDiv) infoDiv.textContent = `${name} — от ${minPrice} ₽`;

    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

// обработка формы заказа
const orderForm = document.getElementById('orderForm');
if (orderForm) {
    const phoneInput = document.getElementById('orderPhone');
    const addressInput = document.getElementById('orderAddress');
    const quantityInput = document.getElementById('orderQuantity');
    const noteInput = document.getElementById('orderNote');
    const serviceIdInput = document.getElementById('orderServiceId');
    const submitBtn = document.getElementById('orderSubmit');

    orderForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const serviceItemId = parseInt(serviceIdInput.value, 10);
        const phone = phoneInput.value.trim();
        const address = addressInput.value.trim();
        const quantity = parseInt(quantityInput.value, 10) || 1;
        const note = noteInput.value.trim();

        if (!phone || !address || !serviceItemId) {
            FormUtils.showError(orderForm, 'Заполните все обязательные поля');
            return;
        }

        try {
            FormUtils.setButtonLoading(submitBtn, true);

            const payload = {
                serviceItemId,
                quantity,
                phone,
                address,
                note
            };

            const resp = await OrderAPI.create(payload);

            FormUtils.showSuccess(orderForm, resp.message || 'Заказ создан');
            // обновить список заказов
            loadOrders();

            setTimeout(() => {
                closeAllModals();
                FormUtils.resetForm(orderForm);
                quantityInput.value = '1';
            }, 1500);
        } catch (error) {
            const msg = error.message || 'Не удалось создать заказ';
            FormUtils.showError(orderForm, msg);
        } finally {
            FormUtils.setButtonLoading(submitBtn, false);
        }
    });
}

// ============ МОИ ЗАКАЗЫ ============

function getOrderStatusText(status) {
    switch (status) {
        case 0:
            return 'Черновик';
        case 1:
            return 'Новый';
        case 2:
            return 'В работе';
        case 3:
            return 'Завершён';
        case 4:
            return 'Отменён';
        default:
            return 'Неизвестно';
    }
}

async function loadOrders() {
    const container = document.getElementById('ordersContainer');
    const unauthBlock = document.getElementById('ordersUnauthorized');
    if (!container) return;

    try {
        const resp = await OrderAPI.getMy();
        const orders = resp.data || [];

        if (unauthBlock) unauthBlock.style.display = 'none';
        container.innerHTML = '';

        if (!orders.length) {
            container.innerHTML = '<p class="section-lead">У вас пока нет заказов.</p>';
            return;
        }

        orders.forEach(order => {
            const status = order.status ?? order.Status;
            const createdAtRaw = order.createdAt || order.CreatedAt;
            const createdAt = createdAtRaw ? new Date(createdAtRaw) : null;

            const card = document.createElement('div');
            card.className = 'card';

            const items = order.items || order.Items || [];
            const firstItem = items[0];
            const serviceName = firstItem
                ? (firstItem.serviceItem?.name || firstItem.ServiceItem?.Name || 'Услуга')
                : 'Услуга';

            card.innerHTML = `
                <h3>Заказ #${order.id || order.Id}</h3>
                <p><strong>Услуга:</strong> ${serviceName}</p>
                <p><strong>Статус:</strong> ${getOrderStatusText(status)}</p>
                <p><strong>Адрес:</strong> ${order.address || order.Address}</p>
                <p><strong>Телефон:</strong> ${order.phone || order.Phone}</p>
                ${createdAt ? `<p><strong>Создан:</strong> ${createdAt.toLocaleString()}</p>` : ''}
            `;

            if (status === 0 || status === 1) {
                const cancelBtn = document.createElement('button');
                cancelBtn.type = 'button';
                cancelBtn.className = 'btn btn-login';
                cancelBtn.style.marginTop = '10px';
                cancelBtn.textContent = 'Отменить заказ';

                cancelBtn.addEventListener('click', async () => {
                    try {
                        FormUtils.setButtonLoading(cancelBtn, true);
                        await OrderAPI.cancel(order.id || order.Id);
                        await loadOrders();
                    } catch (err) {
                        console.error('Ошибка отмены заказа:', err);
                    } finally {
                        FormUtils.setButtonLoading(cancelBtn, false);
                    }
                });

                card.appendChild(cancelBtn);
            }

            container.appendChild(card);
        });
    } catch (error) {
        if (error.status === 401) {
            // не авторизован
            if (unauthBlock) unauthBlock.style.display = 'block';
            container.innerHTML = '';
            return;
        }

        console.error('Ошибка загрузки заказов:', error);
        container.innerHTML = '<p style="color:#fca5a5;">Не удалось загрузить заказы</p>';
    }
}


function updateAuthUI(userData) {
    console.log('Пользователь вошел:', userData);

    const authBtns = document.querySelector('.auth-btns');
    if (authBtns && userData) {
        const roleLabel = userData.role === 'Admin' ? ' (админ)' : '';
        authBtns.innerHTML = `
<span style="color: var(--accent); margin-right: 10px; display: inline-flex; align-items: center; justify-content: center;">
    👤 ${userData.name || userData.email}${roleLabel}
</span>
            <button class="btn btn-login" onclick="logout()">Выйти</button>
        `;
    }

    applyUserToUI(userData);
}


let currentUser = null;
let isAdmin = false;

function applyUserToUI(userData) {
    currentUser = userData;
    isAdmin = !!userData && (userData.role === 'Admin' || userData.role === 'admin');

    const addBtn = document.querySelector('.services-add-btn');
    if (addBtn) {
        addBtn.style.display = isAdmin ? 'inline-flex' : 'none';
    }

    // Пункт меню "Каталог"
    const catalogLi = document.querySelector('.nav-catalog-link');
    if (catalogLi) {
        catalogLi.style.display = userData ? '' : 'none';
    }

    // при смене пользователя перезагрузим услуги, чтобы добавить/убрать админ-кнопки
    if (typeof loadServices === 'function') {
        loadServices();
    }
}

const logoLink = document.querySelector('.logo');
const HOME_URL = logoLink ? logoLink.getAttribute('href') : '/';
// Выход пользователя
async function logout() {
    try {
        await AuthAPI.logout();
        localStorage.removeItem('user');
        currentUser = null;
        isAdmin = false;

        // всегда на главную
        const homeUrl = HOME_URL || '/';
        window.location.href = homeUrl;
    } catch (error) {
        console.error('Ошибка при выходе:', error);
    }
}

// ==================== УСЛУГИ: загрузка и рендер ====================

const servicesGrid = document.getElementById('servicesGrid');
const servicesAddBtn = document.querySelector('.services-add-btn');
const servicesSortSelect = document.getElementById('servicesSort');
const servicesCategorySelect = document.getElementById('servicesCategory');

// страница, на которой мы сейчас рендерим услуги
const servicesPage = servicesGrid?.dataset.page || 'home'; // home | catalog
const isCatalogPage = servicesPage === 'catalog';

// кеш всех услуг, загруженных с бэка
let allServices = [];

if (servicesSortSelect) {
    servicesSortSelect.addEventListener('change', () => {
        const filtered = applyServiceFilters(allServices);
        renderServices(filtered);
    });
}

if (servicesCategorySelect) {
    servicesCategorySelect.addEventListener('change', () => {
        const filtered = applyServiceFilters(allServices);
        renderServices(filtered);
    });
}

async function loadServices() {
    if (!servicesGrid) return;

    servicesGrid.innerHTML = '<p class="section-lead">Загрузка услуг...</p>';

    try {
        // Берём все услуги, без сортировки/фильтров – всё делаем на клиенте
        const response = await ServiceAPI.getAll();
        allServices = response.data || [];

        // На главной странице нас интересуют только активные услуги
        if (servicesPage === 'home') {
            allServices = allServices.filter(s => s.isActive !== false);
        }

        // Для каталога нужно заполнить список категорий
        if (isCatalogPage) {
            populateCategoryFilter(allServices);
        }

        const filtered = applyServiceFilters(allServices);
        renderServices(filtered);
    } catch (error) {
        console.error('Ошибка загрузки услуг', error);
        servicesGrid.innerHTML = '<p class="section-lead">Не удалось загрузить услуги.</p>';
    }
}

function populateCategoryFilter(services) {
    if (!servicesCategorySelect) return;

    const categories = Array.from(
        new Set(
            services
                .map(s => s.category || s.Category)
                .filter(Boolean)
        )
    ).sort();

    servicesCategorySelect.innerHTML = '<option value="">Все категории</option>';

    categories.forEach(cat => {
        const opt = document.createElement('option');
        opt.value = cat;
        opt.textContent = cat;
        servicesCategorySelect.appendChild(opt);
    });
}

function applyServiceFilters(services) {
    if (!services) return [];

    let result = [...services];

    // фильтр по категории (только в каталоге)
    if (isCatalogPage && servicesCategorySelect && servicesCategorySelect.value) {
        const category = servicesCategorySelect.value;
        result = result.filter(s => (s.category || s.Category) === category);
    }

    // сортировка по цене
    if (servicesSortSelect && servicesSortSelect.value) {
        const sort = servicesSortSelect.value;
        result.sort((a, b) => {
            const priceA = a.minPrice ?? a.MinPrice ?? 0;
            const priceB = b.minPrice ?? b.MinPrice ?? 0;
            if (sort === 'priceAsc') return priceA - priceB;
            if (sort === 'priceDesc') return priceB - priceA;
            return 0;
        });
    }

    // на главной: только первые 6 активных
    if (servicesPage === 'home') {
        result = result.filter(s => s.isActive !== false).slice(0, 6);
    }

    return result;
}

function renderServices(services) {
    if (!servicesGrid) return;

    servicesGrid.innerHTML = '';

    if (!services || services.length === 0) {
        servicesGrid.innerHTML = '<p class="section-lead">Услуги пока не добавлены.</p>';
        return;
    }

    // В каталоге админ может редактировать / удалять услуги
    const showAdminActions = isAdmin && isCatalogPage;

    services.forEach(service => {
        // обычный пользователь никогда не видит неактивные
        if (!isAdmin && service.isActive === false) {
            return;
        }

        const card = document.createElement('div');
        card.className = 'service-card';

        if (isAdmin && service.isActive === false) {
            card.classList.add('service-card-inactive');
        }

        const id = service.id || service.Id;
        const name = service.name || service.Name;
        const description = service.description || service.Description;
        const minPrice = service.minPrice ?? service.MinPrice;

        card.innerHTML = `
            ${showAdminActions ? `
            <div class="service-card-actions">
                <button class="service-edit-btn" data-id="${id}" title="Редактировать">✏️</button>
                <button class="service-delete-btn" data-id="${id}" title="Удалить">✖</button>
            </div>` : ''}
            <div class="service-icon">${service.icon || service.Icon || '🔧'}</div>
            <h3 class="service-name">${name}</h3>
            <p>${description}</p>
            <span class="service-price">от ${minPrice} ₽</span>
        `;

        card.style.cursor = 'pointer';

        card.addEventListener('click', (e) => {
            // клики по кнопкам админа не должны открывать детальную страницу
            if (e.target.closest('.service-card-actions')) {
                return;
            }

            if (isCatalogPage) {
                // В каталоге открываем отдельную страницу услуги
                window.location.href = `/Catalog/Service/${id}`;
            } else if (!isAdmin) {
                // На главной – обычный пользователь оформляет заказ через модалку
                openOrderModal(service);
            }
        });

        servicesGrid.appendChild(card);
    });

    if (showAdminActions) {
        attachServiceAdminHandlers();
    }
}




function openServiceModalForCreate() {
    const modal = modals.service;
    if (!modal) return;

    document.getElementById('serviceId').value = '';
    document.getElementById('serviceName').value = '';
    document.getElementById('serviceCategory').value = '';
    document.getElementById('serviceIcon').value = '🔧';
    document.getElementById('serviceMinPrice').value = '';
    document.getElementById('serviceDescription').value = '';
    document.getElementById('serviceIsActive').checked = true;
    document.getElementById('serviceModalTitle').textContent = 'Новая услуга';

    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function openServiceModalForEdit(service) {
    const modal = modals.service;
    if (!modal) return;

    document.getElementById('serviceId').value = service.id;
    document.getElementById('serviceName').value = service.name || '';
    document.getElementById('serviceCategory').value = service.category || '';
    document.getElementById('serviceIcon').value = service.icon || '🔧';
    document.getElementById('serviceMinPrice').value = service.minPrice || 0;
    document.getElementById('serviceDescription').value = service.description || '';
    document.getElementById('serviceIsActive').checked = service.isActive !== false;
    document.getElementById('serviceModalTitle').textContent = 'Редактирование услуги';

    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function attachServiceAdminHandlers() {
    const editButtons = document.querySelectorAll('.service-edit-btn');
    const deleteButtons = document.querySelectorAll('.service-delete-btn');

    editButtons.forEach(btn => {
        btn.addEventListener('click', async () => {
            const id = btn.dataset.id;
            try {
                const response = await ServiceAPI.getById(id);
                const s = response.data;
                openServiceModalForEdit(s);
            } catch (error) {
                console.error('Ошибка получения услуги', error);
            }
        });
    });

    deleteButtons.forEach(btn => {
        btn.addEventListener('click', async () => {
            const id = btn.dataset.id;
            if (!confirm('Удалить услугу?')) return;

            try {
                await ServiceAPI.deleteService(id);
                await loadServices();
            } catch (error) {
                console.error('Ошибка удаления услуги', error);
            }
        });
    });
}

if (servicesAddBtn) {
    servicesAddBtn.addEventListener('click', (e) => {
        e.preventDefault();
        if (!isAdmin) return;
        openServiceModalForCreate();
    });
}


const serviceForm = document.getElementById('serviceForm');
if (serviceForm) {
    const submitBtn = document.getElementById('serviceSubmitBtn');

    serviceForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = document.getElementById('serviceId').value;
        const payload = {
            name: document.getElementById('serviceName').value.trim(),
            category: document.getElementById('serviceCategory').value.trim(),
            icon: document.getElementById('serviceIcon').value.trim() || '🔧',
            minPrice: Number(document.getElementById('serviceMinPrice').value),
            description: document.getElementById('serviceDescription').value.trim(),
            isActive: document.getElementById('serviceIsActive').checked
        };

        if (!payload.name || !payload.category || !payload.description || isNaN(payload.minPrice)) {
            alert('Заполните все поля корректно');
            return;
        }

        try {
            FormUtils.setButtonLoading(submitBtn, true);

            if (id) {
                await ServiceAPI.updateService(id, payload);
            } else {
                await ServiceAPI.createService(payload);
            }

            closeAllModals();
            await loadServices();
        } catch (error) {
            console.error('Ошибка сохранения услуги', error);
            alert(error.message || 'Ошибка сохранения услуги');
        } finally {
            FormUtils.setButtonLoading(submitBtn, false);
        }
    });
}


document.addEventListener('DOMContentLoaded', async () => {
    try {
        const resp = await AuthAPI.checkAuth();
        if (resp.authenticated && resp.data) {
            localStorage.setItem('user', JSON.stringify(resp.data));
            updateAuthUI(resp.data);
        } else {
            localStorage.removeItem('user');
            applyUserToUI(null);
        }
    } catch (error) {
        console.error('Ошибка проверки авторизации', error);
        localStorage.removeItem('user');
        applyUserToUI(null);
    }

    // услуги
    if (typeof loadServices === 'function') {
        loadServices();
    }

    // мои заказы (и на главной, и в каталоге)
    if (typeof loadOrders === 'function') {
        loadOrders();
    }

    const url = new URL(window.location.href);
    const confirmed = url.searchParams.get('emailConfirmed');
    const emailError = url.searchParams.get('emailConfirmError');
    const googleLogin = url.searchParams.get('googleLogin');
    const googleError = url.searchParams.get('googleLoginError');

    if (confirmed === '1') {
        showToast('Почта подтверждена, спасибо!');
        url.searchParams.delete('emailConfirmed');
        window.history.replaceState({}, '', url.pathname + url.search);
    } else if (emailError) {
        showToast(emailError);
        url.searchParams.delete('emailConfirmError');
        window.history.replaceState({}, '', url.pathname + url.search);
    }

    if (googleLogin === '1') {
        showToast('Вы успешно вошли через Google');
        url.searchParams.delete('googleLogin');
        window.history.replaceState({}, '', url.pathname + url.search);
    } else if (googleError) {
        showToast(googleError);
        url.searchParams.delete('googleLoginError');
        window.history.replaceState({}, '', url.pathname + url.search);
    }
});





function showToast(message, type = 'success') {
    const toast = document.getElementById('globalToast');
    if (!toast) return;

    toast.className = 'toast'; // сбрасываем
    if (type === 'error') toast.classList.add('toast-error');
    else toast.classList.add('toast-success');

    toast.textContent = message;

    // показываем
    toast.classList.add('visible');

    clearTimeout(showToast._timer);
    showToast._timer = setTimeout(() => {
        toast.classList.remove('visible');
    }, 4000);
}

window.showToast = showToast;


window.logout = logout;
window.updateAuthUI = updateAuthUI;
