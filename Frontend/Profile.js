window.addEventListener('DOMContentLoaded', async () => {
    const rankDisplay = {
        Novice: { title: 'Новичок', badge: '🪨', color: '#94a3b8' },
        Apprentice: { title: 'Ученик', badge: '🛠️', color: '#cd7f32' },
        Adept: { title: 'Адепт', badge: '🦾', color: '#cbd5e1' },
        Virtuoso: { title: 'Виртуоз', badge: '🌀', color: '#3b82f6' },
        Master: { title: 'Мастер', badge: '⚡', color: '#a855f7' },
        Expert: { title: 'Эксперт', badge: '💎', color: '#10b981' },
        Magister: { title: 'Магистр', badge: '🧪', color: '#ef4444' },
        Archmage: { title: 'Архимаг', badge: '🔮', color: '#8b5cf6' },
        Grandmaster: { title: 'Великий Магистр', badge: '👑', color: '#fbbf24' },
        Legend: { title: 'Легенда', badge: '👑', color: '#e2e8f0' },
        Assistant: { title: 'Ассистент', badge: '📋', color: '#94a3b8' },
        Instructor: { title: 'Инструктор', badge: '📐', color: '#cbd5e1' },
        Lector: { title: 'Лектор', badge: '🎙️', color: '#3b82f6' },
        Curator: { title: 'Куратор', badge: '🛡️', color: '#a855f7' },
        Mentor: { title: 'Ментор', badge: '🧠', color: '#f59e0b' },
        Professor: { title: 'Профессор', badge: '🎓', color: '#10b981' },
        AcademyMagister: { title: 'Магистр Академии', badge: '🏛️', color: '#ef4444' },
        Dean: { title: 'Декан', badge: '⚖️', color: '#8b5cf6' },
        KnowledgeArchitect: { title: 'Архитектор Знаний', badge: '🏗️', color: '#6366f1' }
    };
    const $ = id => document.getElementById(id);
    const apiFetch = typeof window.apiFetch === 'function'
        ? window.apiFetch.bind(window)
        : (url, options = {}) => fetch(url, { credentials: 'include', ...options });

    const params = new URLSearchParams(location.search);
    const requestedId = params.get('id');
    const requestedNick = params.get('nick');
    const pick = (object, ...keys) => {
        for (const key of keys) {
            if (object?.[key] !== undefined && object[key] !== null) return object[key];
        }
        return '';
    };
    const unwrap = payload => payload?.value ?? payload?.Value ?? payload;

    let ownPassport = null;
    try {
        const response = await apiFetch('/api/user/GetMyBusinessPassport');
        if (response.ok) ownPassport = unwrap(await response.json());
    } catch (error) {
        console.error('Не удалось загрузить собственный профиль:', error);
    }

    let isOther = Boolean(requestedId || requestedNick);
    let passport = ownPassport;
    const ownUser = ownPassport?.user ?? ownPassport?.User;
    const ownId = pick(ownUser, 'externalId', 'ExternalId');
    const ownNick = pick(ownPassport, 'userName', 'UserName');
    if (isOther && requestedId && requestedId !== ownId) {
        const query = new URLSearchParams();
        query.set('id', requestedId);
        try {
            const response = await apiFetch(`/api/user/GetBusinessPassport?${query}`);
            if (!response.ok) throw new Error(`Профиль не найден (${response.status}).`);
            passport = unwrap(await response.json());
        } catch (error) {
            console.error('Не удалось загрузить профиль пользователя:', error);
            alert(error.message || 'Не удалось загрузить профиль пользователя.');
            return;
        }
    } else if (isOther && requestedNick && requestedNick !== ownNick) {
        const query = new URLSearchParams({ nick: requestedNick });
        try {
            const response = await apiFetch(`/api/user/GetBusinessPassport?${query}`);
            if (!response.ok) throw new Error(`Профиль не найден (${response.status}).`);
            passport = unwrap(await response.json());
        } catch (error) {
            console.error('Не удалось загрузить профиль пользователя:', error);
            alert(error.message || 'Не удалось загрузить профиль пользователя.');
            return;
        }
    } else {
        isOther = false;
    }

    if (!passport) {
        alert('Не удалось загрузить профиль. Войдите в EduGame и попробуйте снова.');
        return;
    }

    const normalizePassport = value => {
        const user = value.user ?? value.User ?? value;
        const progress = value.levelingProgress ?? value.LevelingProgress ?? {};
        const role = (pick(value, 'userRole', 'UserRole')
            || (pick(user, 'subject', 'Subject') ? 'Teacher' : 'Student')).toLowerCase();
        return {
            id: pick(user, 'externalId', 'ExternalId', 'id', 'Id'),
            userName: pick(value, 'userName', 'UserName', 'nickname', 'Nickname'),
            role,
            firstName: pick(user, 'firstName', 'FirstName'),
            lastName: pick(user, 'lastName', 'LastName'),
            dateOfBirth: String(pick(user, 'dateOfBirth', 'DateOfBirth') || '').slice(0, 10),
            gender: pick(user, 'gender', 'Gender'),
            education: pick(user, 'education', 'Education'),
            subject: pick(user, 'subject', 'Subject'),
            coins: Number(pick(user, 'coinsBalance', 'CoinsBalance')) || 0,
            avatarUrl: pick(user, 'avatarUrl', 'AvatarUrl') || 'images/hamzaBro3.png',
            level: Number(pick(progress, 'level', 'Level')) || 0,
            rank: pick(progress, 'rankKey', 'RankKey'),
            currentXp: Number(pick(progress, 'currentXpInLevel', 'CurrentXpInLevel')) || 0,
            nextXp: Number(pick(progress, 'nextLevelXpRequired', 'NextLevelXpRequired')) || 0
        };
    };

    let data = normalizePassport(passport);
    let snapshot = null;
    let avatarFile = null;
    let previewUrl = null;

    const escapeHtml = value => {
        const element = document.createElement('span');
        element.textContent = value ?? '';
        return element.innerHTML;
    };

    function profileFields() {
        const fields = [
            { key: 'firstName', label: 'Имя', type: 'text' },
            { key: 'lastName', label: 'Фамилия', type: 'text' },
            { key: 'dateOfBirth', label: 'Дата рождения', type: 'date' },
            { key: 'gender', label: 'Пол', type: 'select', options: ['Мужской', 'Женский'] }
        ];
        if (data.role === 'student') fields.push({ key: 'education', label: 'Образование', type: 'text' });
        if (data.role === 'teacher') fields.push({ key: 'subject', label: 'Предмет', type: 'text' });
        return fields;
    }

    function renderFields() {
        const grid = $('fieldsGrid');
        grid.innerHTML = profileFields().map(field => {
            const value = escapeHtml(data[field.key]);
            const control = field.type === 'select'
                ? `<select id="f_${field.key}" disabled>
                    <option value="">—</option>
                    ${field.options.map(option => `<option value="${option}" ${data[field.key] === option ? 'selected' : ''}>${option}</option>`).join('')}
                   </select>`
                : `<input id="f_${field.key}" type="${field.type}" value="${value}" disabled>`;
            return `<div class="p-field">
                <label for="f_${field.key}">${field.label}</label>
                <div class="field-control">
                    ${control}
                    <button type="button" class="field-edit-btn hidden" data-key="${field.key}" title="Изменить" aria-label="Изменить ${field.label}">
                        <i class="fa-solid fa-pen"></i>
                    </button>
                </div>
            </div>`;
        }).join('');

        grid.querySelectorAll('.field-edit-btn').forEach(button => {
            button.addEventListener('click', () => {
                const control = $(`f_${button.dataset.key}`);
                if (!control) return;
                control.disabled = !control.disabled;
                button.classList.toggle('active', !control.disabled);
                if (!control.disabled) control.focus();
            });
        });
    }

    function render() {
        $('profileNick').textContent = data.userName || 'Пользователь';
        $('coinsValue').textContent = data.coins.toLocaleString('ru-RU');
        $('profileAvatar').src = previewUrl || data.avatarUrl;

        const roleBadge = $('roleBadge');
        const isTeacher = data.role === 'teacher';
        roleBadge.className = `role-badge ${isTeacher ? 'teacher' : 'student'}`;
        roleBadge.textContent = isTeacher ? '🏫 Преподаватель' : '🎓 Ученик';

        const rank = rankDisplay[data.rank];
        $('rankBadge').classList.toggle('hidden', !data.rank);
        $('rankBadge').textContent = data.rank
            ? `${rank?.badge || '⭐'} ${rank?.title || data.rank}`
            : '';
        $('rankBadge').style.color = rank?.color || '';
        $('levelBadge').classList.toggle('hidden', !data.level);
        $('levelBadge').textContent = data.level ? `Lvl ${data.level}` : '';
        $('xpBlock').classList.toggle('hidden', !data.level || isTeacher);
        if (data.level && !isTeacher) {
            const progress = data.nextXp > 0 ? Math.min(100, data.currentXp / data.nextXp * 100) : 0;
            $('xpBarFill').style.width = `${progress}%`;
            $('xpBarText').textContent = `${data.currentXp} / ${data.nextXp} XP до следующего уровня`;
        }

        $('editBtn').classList.toggle('hidden', isOther);
        $('avatarEditBtn').classList.toggle('hidden', isOther);
        $('messageBtn').classList.add('hidden');
        $('friendBtn').classList.toggle('hidden', !isOther);
        document.querySelector('.coins-badge')?.classList.toggle('hidden', isOther);
        renderFields();
        setEditing(false);
    }

    function setEditing(enabled) {
        document.querySelectorAll('.field-edit-btn').forEach(button => {
            button.classList.toggle('hidden', !enabled || isOther);
            button.classList.toggle('active', enabled && !isOther);
        });
        profileFields().forEach(field => {
            const control = $(`f_${field.key}`);
            if (control) control.disabled = !enabled || isOther;
        });
        $('editBtn').classList.toggle('hidden', enabled || isOther);
        $('saveBtn').classList.toggle('hidden', !enabled || isOther);
        $('cancelBtn').classList.toggle('hidden', !enabled || isOther);
    }

    function beginEditing() {
        if (snapshot) return;
        snapshot = { ...data };
        setEditing(true);
    }

    $('editBtn').addEventListener('click', beginEditing);
    $('cancelBtn').addEventListener('click', () => {
        if (snapshot) data = snapshot;
        snapshot = null;
        avatarFile = null;
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = null;
        $('avatarInput').value = '';
        render();
    });

    $('avatarEditBtn').addEventListener('click', () => {
        beginEditing();
        $('avatarInput').click();
    });

    $('avatarInput').addEventListener('change', () => {
        const file = $('avatarInput').files?.[0];
        if (!file) return;
        if (!['image/png', 'image/jpeg', 'image/webp'].includes(file.type) || file.size > 5 * 1024 * 1024) {
            alert('Выберите PNG, JPEG или WebP размером не более 5 МБ.');
            $('avatarInput').value = '';
            return;
        }
        avatarFile = file;
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = URL.createObjectURL(file);
        $('profileAvatar').src = previewUrl;
    });

    $('saveBtn').addEventListener('click', async () => {
        const saveButton = $('saveBtn');
        const fields = profileFields();
        fields.forEach(field => {
            const control = $(`f_${field.key}`);
            if (control) data[field.key] = control.value.trim();
        });
        if (!data.firstName || !data.lastName || !data.dateOfBirth) {
            alert('Заполните имя, фамилию и дату рождения.');
            return;
        }

        const formData = new FormData();
        formData.append('FirstName', data.firstName);
        formData.append('LastName', data.lastName);
        formData.append('DateOfBirth', data.dateOfBirth);
        formData.append('Gender', data.gender || '');
        formData.append('Education', data.education || '');
        formData.append('Subject', data.subject || '');
        if (avatarFile) formData.append('Avatar', avatarFile);

        saveButton.disabled = true;
        try {
            const response = await apiFetch('/api/user/UpdateMyProfile', {
                method: 'POST',
                body: formData
            });
            const payload = unwrap(await response.json().catch(() => null));
            if (!response.ok) {
                throw new Error(payload?.error || payload?.title || `Не удалось сохранить профиль (${response.status}).`);
            }

            data = normalizePassport(payload);
            snapshot = null;
            avatarFile = null;
            if (previewUrl) URL.revokeObjectURL(previewUrl);
            previewUrl = null;
            $('avatarInput').value = '';
            render();
        } catch (error) {
            console.error('Не удалось сохранить профиль:', error);
            alert(error.message || 'Не удалось сохранить изменения профиля.');
        } finally {
            saveButton.disabled = false;
        }
    });

    $('friendBtn').addEventListener('click', async () => {
        if (!data.id) return;
        const button = $('friendBtn');
        button.disabled = true;
        try {
            const response = await apiFetch(`/api/friend/SendFriendRequest?receiverId=${encodeURIComponent(data.id)}`, { method: 'POST' });
            const result = await response.json().catch(() => null);
            if (!response.ok || result?.isSuccess === false) {
                throw new Error(result?.errors?.[0] || result?.error || 'Не удалось отправить заявку в друзья.');
            }
            button.innerHTML = '<i class="fa-solid fa-check"></i> Заявка отправлена';
        } catch (error) {
            console.error('Не удалось отправить заявку в друзья:', error);
            alert(error.message);
            button.disabled = false;
        }
    });

    let isLeaving = false;
    function leaveProfile(navigate) {
        if (isLeaving) return;
        isLeaving = true;

        const section = $('profileSection');
        section.classList.remove('animate__fadeIn');
        section.classList.add('animate__fadeOut');

        let timeoutId;
        let hasNavigated = false;
        const finishNavigation = () => {
            if (hasNavigated) return;
            hasNavigated = true;
            clearTimeout(timeoutId);
            section.removeEventListener('animationend', onAnimationEnd);
            navigate();
        };
        const onAnimationEnd = event => {
            if (event.target === section) finishNavigation();
        };
        section.addEventListener('animationend', onAnimationEnd);
        timeoutId = setTimeout(finishNavigation, 1100);
    }

    $('backButton').addEventListener('click', () => leaveProfile(() => {
        if (history.length > 1) history.back();
        else location.href = 'Main.html';
    }));
    document.querySelector('.home-mini-link').addEventListener('click', event => {
        event.preventDefault();
        const destination = event.currentTarget.href;
        leaveProfile(() => { location.href = destination; });
    });

    render();
});