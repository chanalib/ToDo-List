import axios from 'axios';

const apiUrl = "http://localhost:5073"; // הכתובת של ה-API שלך
axios.defaults.baseURL = apiUrl;

// הוספת interceptor לתפיסת שגיאות
axios.interceptors.response.use(
    response => response,
    error => {
        if (error.response && error.response.status === 401) {
            // הפנה לדף הלוגין
            window.location.href = '/login'; // הנח את הכתובת לדף הלוגין שלך
        }
        return Promise.reject(error);
    }
);

export default {
    getTasks: async () => {
        const result = await axios.get(`/items`);
        return result.data;
    },

    addTask: async(name) => {
        console.log('addTask', name);
        try {
            const result = await axios.post(`/items`, { name });
            return result.data;
        } catch (error) {
            console.error("Error adding task:", error.message);
            throw error; // זרוק את השגיאה כדי שתוכל לטפל בה במקום אחר
        }
    },

    setCompleted: async (id, isComplete) => {
        console.log('setCompleted', { id, isComplete });
        try {
            const existingItemResponse = await axios.get(`/items/${id}`);
            const existingItem = existingItemResponse.data;

            const updatedItem = {
                name: existingItem.name,
                isComplete: isComplete
            };

            const result = await axios.put(`/items/${id}`, updatedItem);
            return result.data;
        } catch (error) {
            console.error("Error setting task completion:", error.message);
            throw error;
        }
    },

    deleteTask: async (id) => {
        try {
            await axios.delete(`/items/${id}`);
            console.log('Task deleted successfully');
        } catch (error) {
            console.error("Error deleting task:", error.message);
            throw error;
        }
    }
};
