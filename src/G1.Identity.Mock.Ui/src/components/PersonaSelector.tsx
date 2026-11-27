import { useActionState, useState } from 'react';
import type { PersonaOption } from '../personas';
import styles from '../styles/selector.module.css';

interface PersonaSelectorProps {
    personas: readonly PersonaOption[];
    preview: boolean;
    onContinueAuthorization?: (personaKey: string) => void | Promise<void>;
}

interface SelectionResult {
    personaKey: string;
    success?: string;
    error?: string;
}

export function PersonaSelector({ personas, preview, onContinueAuthorization }: PersonaSelectorProps) {
    const [personaKey, setPersonaKey] = useState('');
    const [result, submitSelection, isPending] = useActionState(handleSelection, null);
    const canContinue = preview || onContinueAuthorization !== undefined;
    const currentResult = result?.personaKey === personaKey ? result : null;

    async function handleSelection(
        _previous: SelectionResult | null,
        formData: FormData,
    ): Promise<SelectionResult | null> {
        const selected = personas.find((persona) => persona.key === formData.get('persona'));
        if (!selected) {
            return { personaKey, error: 'Elige una persona de la lista.' };
        }

        if (preview) {
            return { personaKey: selected.key, success: `Selección de prueba: ${selected.displayName}.` };
        }

        if (!onContinueAuthorization) {
            return null;
        }

        try {
            await onContinueAuthorization(selected.key);
            return null;
        } catch {
            return { personaKey: selected.key, error: 'No se pudo continuar con la autorización. Vuelve a intentarlo.' };
        }
    }

    return (
        <form className={styles.form} action={submitSelection}>
            <div>
                <label className={`form-label ${styles.label}`} htmlFor="persona">Persona de prueba (campo obligatorio)</label>
                <select
                    className={`form-select ${styles.select}`}
                    id="persona"
                    name="persona"
                    value={personaKey}
                    onChange={(event) => setPersonaKey(event.currentTarget.value)}
                    aria-describedby="persona-help"
                    required
                >
                    <option value="" disabled>Elige una persona</option>
                    {personas.map((persona) => (
                        <option key={persona.key} value={persona.key}>{persona.displayName}</option>
                    ))}
                </select>
            </div>
            <p id="persona-help" className={styles.help}>
                {!canContinue && 'Todavía no puedes continuar con la autorización desde este selector.'}
                {canContinue && (preview ? 'Confirma la selección para probar el formulario.' : 'Confirma para continuar con la autorización.')}
            </p>
            {canContinue && (
                <button
                    className={`btn btn-primary ${styles.primaryButton}`}
                    type="submit"
                    aria-disabled={isPending}
                    onClick={(event) => {
                        if (isPending) {
                            event.preventDefault();
                        }
                    }}
                >
                    {preview ? 'Probar selección' : 'Continuar'}
                </button>
            )}
            <div role="status">
                {isPending && <p>Procesando la selección…</p>}
                {!isPending && currentResult?.success && <p>{currentResult.success}</p>}
            </div>
            <div role="alert">
                {!isPending && currentResult?.error && (
                    <p className={`alert alert-danger ${styles.error}`}>{currentResult.error}</p>
                )}
            </div>
        </form>
    );
}
