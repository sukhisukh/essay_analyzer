import React from 'react';

function LoadingSpinner() {

    return(
        <div style={{ textAlign: 'center', padding: '60px' }}>
            {/* Spinning circle using CSS animation */}
            <div style={{
                width: '60px',
                height: '60px',
                border: '6px solid #f0f0f0',
                borderTop: '6px solid #4A90D9',
                borderRadius: '50%',
                animation: 'spin 1s linear infinite',
                margin: '0 auto'
            }} />

            <p style={{ marginTop: '20px', fontSize: '18px', color: '#666' }}>
                Analyzing your essay...
            </p>
            <p style={{ fontSize: '14px', color: '#999' }}>
                This usually takes 5–10 seconds
            </p>

            {/* CSS animation defined inline */}
            <style>{`
                @keyframes spin {
                0%   { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
                }
            `}</style>
        </div>

    );
}
export default LoadingSpinner;