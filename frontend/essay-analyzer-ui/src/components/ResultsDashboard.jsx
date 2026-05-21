import React from 'react';
import ScoreDisplay from './ScoreDisplay';
import CategoryCard from './CategoryCard';

function ResultsDashboard({results, onReset}){
return (
    <div style={{ maxWidth: '800px', margin: '0 auto', padding: '20px' }}>

      {/* Overall Score */}
      <ScoreDisplay 
        score={results.overallScore} 
        summary={results.summary} 
      />

    {/* Benchmark note from real dataset */}
    {results.benchmarkNote && (
      <div style={{
        maxWidth: '600px',
        margin: '15px auto',
        padding: '12px 20px',
        backgroundColor: '#eef4ff',
        border: '1px solid #4A90D9',
        borderRadius: '8px',
        textAlign: 'center',
        fontSize: '14px',
        color: '#1B3A6B'
      }}>
        📊 {results.benchmarkNote}
      </div>
    )}
      <hr style={{ margin: '30px 0', border: 'none', borderTop: '1px solid #eee' }} />

      {/* Category breakdown */}
      <h3 style={{ marginBottom: '15px' }}>📊 Detailed Feedback</h3>
      {results.categories.map((category, index) => (
        <CategoryCard
          key={index}
          name={category.name}
          score={category.score}
          feedback={category.feedback}
        />
      ))}

      <hr style={{ margin: '30px 0', border: 'none', borderTop: '1px solid #eee' }} />

      {/* Strength and improvement */}
      <div style={{ display: 'flex', gap: '15px', marginBottom: '30px' }}>
        
        {/* Top strength */}
        <div style={{
          flex: 1,
          backgroundColor: '#eafaf1',
          border: '1px solid #27ae60',
          borderRadius: '10px',
          padding: '15px'
        }}>
          <h4 style={{ color: '#27ae60', margin: '0 0 8px 0' }}>
            ✅ What You Did Well
          </h4>
          <p style={{ margin: 0, fontSize: '14px' }}>
            {results.topStrength}
          </p>
        </div>

        {/* Top improvement */}
        <div style={{
          flex: 1,
          backgroundColor: '#fef9e7',
          border: '1px solid #f39c12',
          borderRadius: '10px',
          padding: '15px'
        }}>
          <h4 style={{ color: '#f39c12', margin: '0 0 8px 0' }}>
            💡 Top Thing to Improve
          </h4>
          <p style={{ margin: 0, fontSize: '14px' }}>
            {results.topImprovement}
          </p>
        </div>

      </div>

      {/* Try another essay button */}
      <div style={{ textAlign: 'center' }}>
        <button
          onClick={onReset}
          style={{
            padding: '12px 30px',
            fontSize: '16px',
            backgroundColor: '#4A90D9',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer'
          }}
        >
          Analyze Another Essay
        </button>
      </div>

    </div>
  );
}

export default ResultsDashboard;