import React from 'react';

const Toast = ({ type, message, exiting }) => {
  const icons = { success: '✓', error: '✕', info: 'ℹ', warning: '⚡' };
  
  return (
    <div className={`toast toast-${type}${exiting ? ' exiting' : ''}`}>
      <span style={{ fontWeight: 700, fontSize: '16px' }}>{icons[type]}</span>
      {message}
    </div>
  );
};

export default Toast;
