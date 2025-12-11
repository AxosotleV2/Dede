// wwwroot/js/api-client.js

/**
 * API Client для работы с backend через fetch
 */
class ApiClient {
    constructor(baseUrl = '/api') {
        this.baseUrl = baseUrl;
    }

    /**
     * Универсальный метод для отправки запросов
     */
    async request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;

        const config = {
            method: options.method || 'GET',
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        };

        // body только для методов != GET/HEAD
        if (options.body && config.method !== 'GET' && config.method !== 'HEAD') {
            config.body = JSON.stringify(options.body);
        }

        try {
            const response = await fetch(url, config);
            const data = await response.json().catch(() => ({}));

            if (!response.ok) {
                throw {
                    status: response.status,
                    message: data.message || 'Произошла ошибка',
                    data: data
                };
            }

            return data;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    // GET запрос с query-параметрами
    async get(endpoint, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `${endpoint}?${queryString}` : endpoint;
        return this.request(url, {method: 'GET'});
    }

    async post(endpoint, body = {}) {
        return this.request(endpoint, {
            method: 'POST',
            body: body
        });
    }

    async put(endpoint, body = {}) {
        return this.request(endpoint, {
            method: 'PUT',
            body: body
        });
    }

    async delete(endpoint) {
        return this.request(endpoint, {method: 'DELETE'});
    }

    async patch(endpoint, body = {}) {
        return this.request(endpoint, {
            method: 'PATCH',
            body: body
        });
    }
}

// Глобальный экземпляр клиента
const api = new ApiClient();

/**
 * Утилиты для работы с формами и UI
 */
let FormUtils = {
    showError(form, message) {
        this.clearMessages(form);
        const errorDiv = document.createElement('div');
        errorDiv.className = 'form-message form-error';
        errorDiv.textContent = message;
        form.insertBefore(errorDiv, form.firstChild);
        setTimeout(() => errorDiv.remove(), 5000);
    },

    showSuccess(form, message) {
        this.clearMessages(form);
        const successDiv = document.createElement('div');
        successDiv.className = 'form-message form-success';
        successDiv.textContent = message;
        form.insertBefore(successDiv, form.firstChild);
        setTimeout(() => successDiv.remove(), 5000);
    },

    clearMessages(form) {
        form.querySelectorAll('.form-message').forEach(msg => msg.remove());
    },

    setButtonLoading(button, isLoading) {
        if (!button) return;
        if (isLoading) {
            button.disabled = true;
            button.dataset.originalText = button.textContent;
            button.textContent = 'Загрузка...';
        } else {
            button.disabled = false;
            if (button.dataset.originalText) {
                button.textContent = button.dataset.originalText;
            }
        }
    },

    getFormData(form) {
        const formData = new FormData(form);
        const data = {};
        for (let [key, value] of formData.entries()) {
            data[key] = value;
        }
        return data;
    },

    resetForm(form) {
        form.reset();
        this.clearMessages(form);
    }
};

/**
 * API-обёртки
 */
let AuthAPI = {
    async login(email, password) {
        return await api.post('/auth/login', {email, password});
    },

    async register(name, email, phone, password, confirmPassword) {
        return await api.post('/auth/register', {
            name,
            email,
            phone,
            password,
            confirmPassword
        });
    },

    async logout() {
        return await api.post('/auth/logout');
    },

    async checkAuth() {
        return await api.get('/auth/check');
    }
};

const ContactAPI = {
    async send(name, email, subject, message) {
        return api.post('/contact', {
            name,
            email,
            subject,
            message
        });
    }
};

// пока мастера у нас заглушка, пусть остаётся
const MasterAPI = {
    async getAll(filters = {}) {
        return await api.get('/masters', filters);
    },

    async getById(id) {
        return await api.get(`/masters/${id}`);
    },

    async search(query) {
        return await api.get('/masters/search', {q: query});
    }
};

// Услуги (карточки на главной)
const ServiceAPI = {
    // Получить все услуги
    async getAll(filters = {}) {
        return await api.get('/services', filters);
    },

    // Получить услугу по ID
    async getById(id) {
        return await api.get(`/services/${id}`);
    },

    // Создать заказ
    async createOrder(serviceId, masterId, details) {
        return await api.post('/orders', {
            serviceId,
            masterId,
            details
        });
    },

    // ===== Админские методы =====
    async createService(payload) {
        return await api.post('/services', payload);
    },

    async updateService(id, payload) {
        return await api.put(`/services/${id}`, payload);
    },

    async deleteService(id) {
        return await api.delete(`/services/${id}`);
    }
};


// Заказы пользователя
const OrderAPI = {
    async create(order) {
        // order: { serviceItemId, quantity, phone, address, note }
        return await api.post('/orders', order);
    },

    async getMy() {
        return await api.get('/orders/my');
    },

    async cancel(orderId) {
        return await api.post(`/orders/${orderId}/cancel`);
    }
};

// Экспорт в window
window.api = api;
window.AuthAPI = AuthAPI;
window.ContactAPI = ContactAPI;
window.MasterAPI = MasterAPI;
window.ServiceAPI = ServiceAPI;
window.OrderAPI = OrderAPI;
window.FormUtils = FormUtils;
