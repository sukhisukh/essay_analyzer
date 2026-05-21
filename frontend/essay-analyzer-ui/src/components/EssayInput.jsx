import React, { useState } from 'react';  // ← ADD THIS LINE

function EssayInput({ onSubmit }) {

  // Now use useState directly instead of React.useState
  const [essay, setEssay] = useState('');

  // Count words live as student types
  const wordCount = essay.trim() === '' ? 0 : essay.trim().split(/\s+/).length;

  // Called when student clicks Analyze button
  const handleSubmit = () => {
    onSubmit(essay);
  };

  return (
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      
      <h2>Paste Your Essay Below</h2>

      <textarea
        rows={15}
        style={{ width: '100%', fontSize: '14px', padding: '10px' }}
        placeholder="Paste your essay here..."
        value={essay}
        onChange={(e) => setEssay(e.target.value)}
      />

      {/* Live word count */}
      <p style={{ color: wordCount >= 50 ? 'green' : 'red' }}>
        Word count: {wordCount} {wordCount < 50 ? '(minimum 50 words required)' : '✓'}
      </p>

      {/* Button — disabled until 50+ words */}
      <button
        onClick={handleSubmit}
        disabled={wordCount < 50}
        style={{
          padding: '10px 20px',
          fontSize: '16px',
          backgroundColor: wordCount >= 50 ? '#4A90D9' : '#cccccc',
          color: 'white',
          border: 'none',
          borderRadius: '5px',
          cursor: wordCount >= 50 ? 'pointer' : 'not-allowed'
        }}
      >
        Analyze My Essay
      </button>

    </div>
  );
}

export default EssayInput;