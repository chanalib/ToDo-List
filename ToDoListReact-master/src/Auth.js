import React, { useState } from 'react';
import axios from 'axios';

function Auth() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    const register = async () => {
        try {
            await axios.post('http://localhost:5073/register', { username, password });
            alert("User registered!");
        } catch (error) {
            alert("Error registering user: " + error.response.data);
        }
    };

    const login = async () => {
        try {
            const response = await axios.post('http://localhost:5073/login', { username, password });
            localStorage.setItem('token', response.data.token);
            alert("Logged in!");
        } catch (error) {
            alert("Error logging in: " + error.response.data);
        }
    };

    return (
        <div>
            <h2>Register</h2>
            <input placeholder="Username" onChange={(e) => setUsername(e.target.value)} />
            <input type="password" placeholder="Password" onChange={(e) => setPassword(e.target.value)} />
            <button onClick={register}>Register</button>
            <h2>Login</h2>
            <button onClick={login}>Login</button>
        </div>
    );
}

export default Auth;
