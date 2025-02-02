import axios from 'axios';

// הגדרת כתובת ה-API כ-default
const apiUrl = "https://localhost:7271"; // ודא שהכתובת היא זו
axios.defaults.baseURL = apiUrl;

// הוספת interceptor לתפיסת שגיאות
axios.interceptors.response.use(
  response => response,
  error => {
    console.error('Error occurred:', error); // רושם את השגיאה ללוג
    return Promise.reject(error);
  }
);
axios.interceptors.response.use(
  response => response,
  error => {
      if (error.response && error.response.status === 401) {
          window.location.href = '/login'; // הפנה לדף התחברות
      }
      return Promise.reject(error);
  }
);

export default {
  getTasks: async () => {
    const result = await axios.get('/items');    
    return result.data;
  },

  addTask: async (name) => {
    console.log('addTask', name);
    const result = await axios.post('/items', { name });
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    console.log('setCompleted', { id, isComplete });
    const result = await axios.put(`/items/${id}`, { isComplete });
    return result.data;
  },

  deleteTask: async (id) => {
    console.log('deleteTask', id);
    await axios.delete(`/items/${id}`);
  }
};
