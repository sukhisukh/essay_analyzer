import React from 'react';

function CategoryCard({ name, score, feedback }) {

  const getColor = (s) => {
    if (s === 4) return '#27ae60';
    if (s === 3) return '#f39c12';
    if (s === 2) return '#e67e22';
    return '#e74c3c';
  };

  return (
    <div style={{
      border: '1px solid #e0e0e0',
      borderRadius: '10px',
      padding: '20px',
      marginBottom: '15px',
      borderLeft: `5px solid ${getColor(score)}`
    }}>

      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '10px'
      }}>
        <h3 style={{ margin: 0, fontSize: '16px' }}>{name}</h3>
        <span style={{
          backgroundColor: getColor(score),
          color: 'white',
          padding: '4px 12px',
          borderRadius: '20px',
          fontWeight: 'bold',
          fontSize: '14px'
        }}>
          {score}/4
        </span>
      </div>

      <div style={{
        backgroundColor: '#f0f0f0',
        borderRadius: '10px',
        height: '8px',
        marginBottom: '12px'
      }}>
        <div style={{
          backgroundColor: getColor(score),
          width: `${(score / 4) * 100}%`,
          height: '8px',
          borderRadius: '10px'
        }} />
      </div>

      <p style={{ margin: 0, fontSize: '14px', color: '#555', lineHeight: '1.5' }}>
        💡 {feedback}
      </p>

    </div>
  );
}

export default CategoryCard;