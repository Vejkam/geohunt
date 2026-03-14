// hooks/useAuth.ts
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export const useAuth = (redirectOnFail = true) => {
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [loggedIn, setLoggedIn] = useState(false);
  const [loading, setLoading] = useState(true);
  const [userId, setUserId] = useState('');
  const [createdAt, setCreatedAt] = useState('');

  useEffect(() => {
    const checkLogin = async () => {
      try {
        const res = await fetch('/api/User/me', {
          method: 'GET',
          credentials: 'include'
        });
        if (res.ok) {
          const data = await res.json();
          setLoggedIn(true);
          setUsername(data.username);
          setUserId(data.id);
          setCreatedAt(data.createdAt);
          const date = new Date(data.createdAt);
          const formattedDate = date.toISOString().split('T')[0];
          setCreatedAt(formattedDate);
        } else {
          setLoggedIn(false);
          if (redirectOnFail) {
            navigate('/login', { replace: true });
          }
        }
      } catch (error) {
        console.error('Error checking login status:', error);
        setLoggedIn(false);
        if (redirectOnFail) {
          navigate('/login', { replace: true });
        }
      } finally {
        setLoading(false);
      }
    };
    checkLogin();
  }, [navigate, redirectOnFail]);

  return { username, loggedIn, loading, userId, createdAt };
};