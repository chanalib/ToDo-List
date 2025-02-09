import axios from 'axios';

const apiUrl =  process.env.REACT_APP_API_URL;
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
        const result = await axios.get('/items');
        return result.data;
    },

    addTask: async(name) => {
        console.log('addTask', name);
        try {
            const result = await axios.post('/items', { name });
            return result.data;
        } catch (error) {
            console.error("Error adding task:", error.message);
            throw error; // זרוק את השגיאה כדי שתוכל לטפל בה במקום אחר
        }
    },

    setCompleted: async (id, isComplete,name) => {
        await axios.put(`/items/${id}`, {name:name,isComplete: isComplete });
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
