import React, { useState } from 'react';
import axios from 'axios';

function Auth() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    const register = async () => {
        await axios.post('/register', { username, password });
        alert("User registered!");
    };

    const login = async () => {
        const response = await axios.post('/login', { username, password });
        localStorage.setItem('token', response.data.token);
        alert("Logged in!");
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
