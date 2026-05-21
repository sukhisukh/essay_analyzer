import React from 'react';
function ScoreDisplay({ score, summary }) {

  // Color based on overall score 1-6
  const getScoreColor = (score) => {
    if (score >= 5) return '#27ae60';  // green
    if (score >= 3) return '#f39c12';  // orange
    return '#e74c3c';                  // red
  };
// Label based on score
  const getScoreLabel = (score) => {
    if (score === 6) return 'Outstanding ⭐';
    if (score === 5) return 'Strong ✅';
    if (score === 4) return 'Proficient 👍';
    if (score === 3) return 'Developing 📈';
    if (score === 2) return 'Emerging 🌱';
    return 'Beginning 💪';
  };

  return (
    <div style={{ textAlign: 'center', padding: '30px 20px' }}>
      
      {/* Big score circle */}
      <div style={{
        width: '120px',
        height: '120px',
        borderRadius: '50%',
        backgroundColor: getScoreColor(score),
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        margin: '0 auto 15px auto',
        boxShadow: '0 4px 15px rgba(0,0,0,0.2)'
      }}>
        <span style={{ 
          fontSize: '42px', 
          fontWeight: 'bold', 
          color: 'white',
          lineHeight: 1
        }}>
          {score}
        </span>
        <span style={{ 
          fontSize: '12px', 
          color: 'white',
          opacity: 0.9
        }}>
          out of 6
        </span>
      </div>

      {/* Score label */}
      <h2 style={{ 
        color: getScoreColor(score),
        marginBottom: '15px',
        fontSize: '22px'
      }}>
        {getScoreLabel(score)}
      </h2>

      {/* AI summary */}
      <p style={{
        maxWidth: '600px',
        margin: '0 auto',
        fontSize: '16px',
        color: '#555',
        lineHeight: '1.6',
        backgroundColor: '#f9f9f9',
        padding: '15px',
        borderRadius: '8px'
      }}>
        {summary}
      </p>

    </div>
  );
}

export default ScoreDisplay;